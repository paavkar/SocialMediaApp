using Microsoft.Azure.Cosmos;
using SocialMediaApp.Server.CosmosDb;
using SocialMediaApp.Server.Models;

namespace SocialMediaApp.Server.Services
{
    public class ChangeFeedService : IHostedService
    {
        private readonly CosmosClient client;
        private readonly string DatabaseName = "SocialApp";
        private readonly string UserContainerName = "UserAccounts";
        private readonly string PostContainerName = "Posts";
        private readonly Container? UserContainer;
        private readonly Container? PostContainer;
        private ChangeFeedProcessor? changeFeedProcessor;

        public ChangeFeedService(IConfiguration configuration)
        {
            CosmosDbSettings settings = configuration.GetSection(CosmosDbSettings.Section).Get<CosmosDbSettings>() ?? throw new ArgumentNullException(nameof(CosmosDbSettings));
            var serializationOptions = new CosmosSerializationOptions
            {
                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase,
            };

            client = new CosmosClient(settings.ConnectionString, new CosmosClientOptions()
            {
                SerializerOptions = serializationOptions
            });
            Database database = client.GetDatabase(DatabaseName);
            UserContainer = database.GetContainer(UserContainerName);
            PostContainer = database.GetContainer(PostContainerName);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Container leaseContainer = client.GetContainer(DatabaseName, "leaseContainer");
            changeFeedProcessor = client.GetContainer(DatabaseName, UserContainerName)
                .GetChangeFeedProcessorBuilder<UserAccount>(processorName: "ChangeFeed", onChangesDelegate: HandleChangesAsync)
                    .WithInstanceName($"UsersPosts{Guid.CreateVersion7()}")
                    .WithLeaseContainer(leaseContainer)
                    .Build();

            Console.WriteLine("Starting Change Feed Processor for Users and Posts...");
            await changeFeedProcessor.StartAsync();
            Console.WriteLine("Change Feed Processor started for Users and Posts.");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (changeFeedProcessor != null)
            {
                await changeFeedProcessor.StopAsync();
            }
        }

        private async Task HandleChangesAsync(ChangeFeedProcessorContext context, IReadOnlyCollection<dynamic> changed, CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine($"Started handling changes for lease {context.LeaseToken}...");
                Console.WriteLine($"Change Feed request consumed {context.Headers.RequestCharge} RU.");
                // SessionToken if needed to enforce Session consistency on another client instance
                Console.WriteLine($"SessionToken ${context.Headers.Session}");

                // We may want to track any operation's Diagnostics that took longer than some threshold
                if (context.Diagnostics.GetClientElapsedTime() > TimeSpan.FromSeconds(1))
                {
                    Console.WriteLine($"Change Feed request took longer than expected. Diagnostics:" + context.Diagnostics.ToString());
                }

                var changendUsers = changed.Where(au => au.Type == "UserAccount").ToList();

                foreach (UserAccount userAccount in changendUsers)
                {
                    var userResponse = await UserContainer!.ReadItemAsync<UserAccount>(userAccount.Id, new PartitionKey("User"), cancellationToken: cancellationToken);
                    UserAccount user = userResponse.Resource;

                    if (!user.ChangeFeed)
                    {
                        continue;
                    }

                    Console.WriteLine($"Detected operation for item with id {userAccount.Id}.");

                    await UserContainer.PatchItemAsync<UserAccount>(userAccount.Id, new PartitionKey("User"),
                            patchOperations: new[]
                            {
                                PatchOperation.Replace<bool>("/changeFeed", false)
                            },
                            cancellationToken: cancellationToken);

                    var usersToUpdate = await GetFollowingOrFollowerUsersAsync(user.Id);
                    var usersToUpdate2 = await GetBookmarkedPostsAuthorsUsersAsync(user.Id);
                    var postsToUpdate = await GetPostsWithCertainAuthorAsync(user.Id);
                    var quotesToUpdate = await GetQuotesWithCertainQuotedAuthorAsync(user.Id);
                    var repliesToUpdate = await GetRepliesWithCertainParentAuthorAsync(user.Id);

                    foreach (var userToUpdate in usersToUpdate)
                    {
                        foreach (var follower in userToUpdate.Followers)
                        {
                            if (follower.Id == user.Id)
                            {
                                follower.DisplayName = user.DisplayName;
                                follower.UserName = user.UserName;
                                follower.Description = user.Description;
                                follower.ProfilePicture = user.ProfilePicture;
                            }
                        }

                        foreach (var following in userToUpdate.Following)
                        {
                            if (following.Id == user.Id)
                            {
                                following.DisplayName = user.DisplayName;
                                following.UserName = user.UserName;
                                following.Description = user.Description;
                                following.ProfilePicture = user.ProfilePicture;
                            }
                        }
                        await UserContainer.PatchItemAsync<UserAccount>(userToUpdate.Id, new PartitionKey("User"),
                            patchOperations: new[]
                            {
                                PatchOperation.Replace("/followers", userToUpdate.Followers),
                                PatchOperation.Replace("/following", userToUpdate.Following)
                            },
                            cancellationToken: cancellationToken);
                    }

                    foreach (var userToUpdate in usersToUpdate2)
                    {
                        foreach (var bookmark in userToUpdate.Bookmarks)
                        {
                            if (bookmark.Author.Id == user.Id)
                            {
                                bookmark.Author.DisplayName = user.DisplayName;
                                bookmark.Author.UserName = user.UserName;
                                bookmark.Author.Description = user.Description;
                                bookmark.Author.ProfilePicture = user.ProfilePicture;
                            }
                        }
                        await UserContainer.PatchItemAsync<UserAccount>(userToUpdate.Id, new PartitionKey("User"),
                            patchOperations: new[]
                            {
                                PatchOperation.Replace("/bookmarks", userToUpdate.Bookmarks)
                            },
                            cancellationToken: cancellationToken);
                    }

                    foreach (var postToUpdate in postsToUpdate)
                    {
                        await PostContainer!.PatchItemAsync<Post>(postToUpdate.Id, new PartitionKey($"Post-{user.Id}"),
                            patchOperations: new[]
                            {
                                PatchOperation.Replace("/author/displayName", user.DisplayName),
                                PatchOperation.Replace("/author/userName", user.UserName),
                                PatchOperation.Replace("/author/description", user.Description),
                                PatchOperation.Replace("/author/profilePicture", user.ProfilePicture)
                            },
                            cancellationToken: cancellationToken);
                    }

                    foreach (var quoteToUpdate in quotesToUpdate)
                    {
                        await PostContainer!.PatchItemAsync<Post>(quoteToUpdate.Id, new PartitionKey($"Post-{quoteToUpdate.Author.Id}"),
                            patchOperations: new[]
                            {
                                PatchOperation.Replace("/quotedPost/author/displayName", user.DisplayName),
                                PatchOperation.Replace("/quotedPost/author/userName", user.UserName),
                                PatchOperation.Replace("/quotedPost/author/description", user.Description),
                                PatchOperation.Replace("/quotedPost/author/profilePicture", user.ProfilePicture)
                            },
                            cancellationToken: cancellationToken);
                    }

                    foreach (var replyToUpdate in repliesToUpdate)
                    {
                        await PostContainer!.PatchItemAsync<Post>(replyToUpdate.Id, new PartitionKey($"Post-{replyToUpdate.Author.Id}"),
                            patchOperations: new[]
                            {
                                PatchOperation.Replace("/parentPost/author/displayName", user.DisplayName),
                                PatchOperation.Replace("/parentPost/author/userName", user.UserName),
                                PatchOperation.Replace("/parentPost/author/description", user.Description),
                                PatchOperation.Replace("/parentPost/author/profilePicture", user.ProfilePicture)
                            },
                            cancellationToken: cancellationToken);
                    }
                }
                Console.WriteLine("Finished handling changes.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public async Task<List<UserAccount>> GetFollowingOrFollowerUsersAsync(string userId)
        {
            var parameterizedQuery = new QueryDefinition
                ("SELECT * FROM UserAccounts ua WHERE ARRAY_CONTAINS(ua.followers, { 'id': @UserId }, true) OR ARRAY_CONTAINS(ua.following, { 'id': @UserId }, true)")
                .WithParameter("@UserId", userId);

            var users = await GetUsersFromFeedIterator(parameterizedQuery);

            return users;
        }

        public async Task<List<UserAccount>> GetBookmarkedPostsAuthorsUsersAsync(string userId)
        {
            var parameterizedQuery = new QueryDefinition
                ("SELECT * FROM UserAccounts ua WHERE ARRAY_CONTAINS(ua.bookmarks, { 'author.id': @UserId }, true)")
                .WithParameter("@UserId", userId);

            var users = await GetUsersFromFeedIterator(parameterizedQuery);

            return users;
        }

        public async Task<List<UserAccount>> GetUsersFromFeedIterator(QueryDefinition parameterizedQuery)
        {
            try
            {
                using FeedIterator<UserAccount> feedIterator = UserContainer!.GetItemQueryIterator<UserAccount>(
                    queryDefinition: parameterizedQuery);

                List<UserAccount> matchingUsers = [];

                while (feedIterator.HasMoreResults)
                {
                    FeedResponse<UserAccount> users = await feedIterator.ReadNextAsync();
                    foreach (var user in users)
                    {
                        matchingUsers.Add(user);
                    }
                }
                return matchingUsers;
            }
            catch (Exception)
            {
                return [];
            }
        }

        public async Task<List<Post>> GetPostsWithCertainAuthorAsync(string userId)
        {
            var parameterizedQuery = new QueryDefinition
                ("SELECT * FROM Posts p WHERE p.author.id = @UserId")
                .WithParameter("@UserId", userId);

            var posts = await GetPostsFromFeedIteratorAsync(parameterizedQuery);

            return posts;
        }

        public async Task<List<Post>> GetQuotesWithCertainQuotedAuthorAsync(string userId)
        {
            var parameterizedQuery = new QueryDefinition
                ("SELECT * FROM Posts p WHERE p.quotedPost.author.id = @UserId")
                .WithParameter("@UserId", userId);

            var posts = await GetPostsFromFeedIteratorAsync(parameterizedQuery);

            return posts;
        }

        public async Task<List<Post>> GetRepliesWithCertainParentAuthorAsync(string userId)
        {
            var parameterizedQuery = new QueryDefinition
                ("SELECT * FROM Posts p WHERE p.parentPost.author.id = @UserId")
                .WithParameter("@UserId", userId);

            var posts = await GetPostsFromFeedIteratorAsync(parameterizedQuery);

            return posts;
        }

        public async Task<List<Post>> GetPostsFromFeedIteratorAsync(QueryDefinition parameterizedQuery)
        {
            try
            {
                using FeedIterator<Post> feedIterator = PostContainer!.GetItemQueryIterator<Post>(
                                queryDefinition: parameterizedQuery);

                List<Post> matchingPosts = [];

                while (feedIterator.HasMoreResults)
                {
                    FeedResponse<Post> posts = await feedIterator.ReadNextAsync();
                    foreach (var post in posts)
                    {
                        matchingPosts.Add(post);
                    }
                }
                return matchingPosts;
            }
            catch (Exception)
            {
                return [];
            }
        }
    }
}
