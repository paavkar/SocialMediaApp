export interface AccountSettings {
    signInRequired: boolean;
    isPrivate: boolean;
}

export interface Author {
    id: string;
    displayName: string;
    description: string;
    userName: string;
    followers?: Author[];
    following?: Author[];
}

export interface LoginDTO {
    emailOrUserName: string;
    password: string;
}

export interface Post {
    id: string;
    text: string;
    likeCount: number;
    repostCount: number;
    quoteCount: number;
    replyCount: number;
    accountsLiked: Author[];
    accountsReposted: Author[];
    quotes: Post[];
    replies: Post[];
    createdAt: Date;
    author: Author;
    labels: string[];
    langs: string[];
    quotedPost: Post;
    parentPost: Post;
    previousVersions: Post[];
    bookmarkCount: number;
    isPinned: boolean;

    repostedBy: Author;
    repostedAt: Date;
}

export interface User {
    id: string;
    displayName: string;
    description: string;
    userName: string;
    email: string;
    likedPosts: Post[];
    repostedPosts: Post[];
    bookmarks: Post[];
    accountSettings: AccountSettings;
    following?: Author[];
    followers?: Author[];
    followRequests: Author[];
}

export interface RegisterDTO {
    email: string;
    displayName: string;
    userName: string;
    password: string;
}