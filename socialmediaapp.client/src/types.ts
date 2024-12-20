export type AccountSettings = {
    signInRequired: boolean;
    isPrivate: boolean;
}

export type Author = {
    id: string;
    displayName: string;
    userName: string;
}

export type LoginDTO = {
    emailOrUserName: string;
    password: string;
}

export type Post = {
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

export type User = {
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

export type RegisterDTO = {
    displayName: string;
    userName: string;
    email: string;
    password: string;
}