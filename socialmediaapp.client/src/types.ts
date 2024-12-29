export interface AccountSettings {
    signInRequired: boolean;
    isPrivate: boolean;
}

export interface Author {
    id: string;
    profilePicture: string;
    displayName: string;
    description: string;
    userName: string;
    followers?: Author[];
    following?: Author[];
}

export interface Embed {
    embedType: EmbedType;
    images?: Media[];
    videos?: Media[];
    externalLink?: ExternalLink;
}

export interface AspectRatio {
    height: number;
    width: number;
}

export interface ExternalLink {
    externalLinkUri: string;
    externalLinkTitle: string;
    externalLinkDescription: string;
    externalLinkThumbnail: string;
}

export enum EmbedType {
    None = 0,
    Image,
    Images,
    Video,
    ExternalLink
}

export interface Media {
    altText: string;
    aspectRatio: AspectRatio;
    filePath?: string;
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
    thread: Post[];
    authorThread: Post[];
    createdAt: Date;
    author: Author;
    labels: string[];
    langs: string[];
    quotedPost?: Post;
    parentPost?: Post;
    previousVersions: Post[];
    bookmarkCount: number;
    isPinned: boolean;
    embed: Embed;

    repostedBy: Author;
    repostedAt: Date;
}

export interface User {
    id: string;
    profilePicture: string;
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
    followRequests?: Author[];
}

export interface RegisterDTO {
    email: string;
    displayName: string;
    userName: string;
    password: string;
}