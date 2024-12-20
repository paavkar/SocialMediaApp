export interface AccountSettings {
    signInRequired: boolean;
    isPrivate: boolean;
}

export interface Author {
    id: string;
    displayName: string;
    userName: string;
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
    quotes: Post[];
    replies: Post[];
    CreatedAt: Date;
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
    userName: string;
    email: string;
    likedPosts: Post[];
    repostedPosts: Post[];
    bookmarks: Post[];
    accountSetting: AccountSettings;
    following: Author[];
    followers: Author[];
}

export interface RegisterDTO {
    displayName: string;
    userName: string;
    email: string;
    password: string;
}