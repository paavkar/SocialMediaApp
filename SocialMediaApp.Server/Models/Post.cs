﻿using Newtonsoft.Json;
using SocialMediaApp.Server.Models.DTOs;

namespace SocialMediaApp.Server.Models
{
    public class Post
    {
        [JsonProperty(PropertyName = "id")]
        public string? Id { get; set; }

        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        [JsonProperty(PropertyName = "likeCount")]
        public int LikeCount { get; set; }

        [JsonProperty(PropertyName = "repostCount")]
        public int RepostCount { get; set; }

        [JsonProperty(PropertyName = "quoteCount")]
        public int QuoteCount { get; set; }

        [JsonProperty(PropertyName = "replyCount")]
        public int ReplyCount { get; set; }

        [JsonProperty(PropertyName = "replyIds")]
        public List<string>? ReplyIds { get; set; }

        [JsonProperty(PropertyName = "replies")]
        public List<PostDTO>? Replies { get; set; }

        [JsonProperty(PropertyName = "accountsLiked")]
        public List<Author>? AccountsLiked { get; set; }

        [JsonProperty(PropertyName = "accountsReposted")]
        public List<Author>? AccountsReposted { get; set; }

        [JsonProperty(PropertyName = "createdAt")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty(PropertyName = "author")]
        public Author Author { get; set; }

        [JsonProperty(PropertyName = "labels")]
        public List<string>? Labels { get; set; }

        [JsonProperty(PropertyName = "langs")]
        public List<string> Langs { get; set; }

        [JsonProperty(PropertyName = "quotedPost")]
        public Post? QuotedPost { get; set; }

        [JsonProperty(PropertyName = "parentPost")]
        public Post? ParentPost { get; set; }

        [JsonProperty(PropertyName = "previousVersions")]
        public List<Post>? PreviousVersions { get; set; }

        [JsonProperty(PropertyName = "bookmarkCount")]
        public int BookmarkCount { get; set; }

        [JsonProperty(PropertyName = "isPinned")]
        public bool IsPinned { get; set; } = false;

        [JsonProperty(PropertyName = "embed")]
        public Embed Embed { get; set; }

        [JsonProperty(PropertyName = "partitionKey")]
        public string? PartitionKey { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string? Type { get; set; } = "Post";

        public PostDTO ToPostDTO(UserAccount author)
        {
            return new PostDTO
            {
                Id = this.Id,
                Text = this.Text,
                LikeCount = this.LikeCount,
                RepostCount = this.RepostCount,
                QuoteCount = this.QuoteCount,
                ReplyCount = this.ReplyCount,
                ReplyIds = this.ReplyIds,
                AccountsLiked = this.AccountsLiked,
                AccountsReposted = this.AccountsReposted,
                CreatedAt = this.CreatedAt,
                Author = author.ToAuthorDTO(),
                Labels = this.Labels,
                Langs = this.Langs,
                QuotedPost = this.QuotedPost,
                ParentPost = this.ParentPost,
                PreviousVersions = this.PreviousVersions,
                BookmarkCount = this.BookmarkCount,
                IsPinned = this.IsPinned,
                Embed = this.Embed,
                PartitionKey = this.PartitionKey
            };
        }
    }
}
