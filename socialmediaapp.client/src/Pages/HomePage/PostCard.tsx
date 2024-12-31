import { EmbedType, Post, User } from "../../types"
import { RootState } from "../../state";
import { useSelector } from "react-redux";
import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useDispatch } from "react-redux";
import { setUser } from "../../state";
import { ReplyModal } from "./ReplyModal";
import { QuoteModal } from "./QuoteModal";

type PostProps = {
    post: Post;
}

export const PostCard = ({ post }: PostProps) => {
    const [currentDate, _setCurrentDate] = useState<Date>(new Date())
    const [uPost, setUPost] = useState(post)
    const [showModal, setShowModal] = useState(false)
    const [showQuoteModal, setShowQuoteModal] = useState(false)
    const token = useSelector<RootState, string | null>((state) => state.token);
    const user = useSelector<RootState, User | null>((state) => state.user);
    
    const dispatch = useDispatch();
    const navigate = useNavigate();
    
    async function likePost() {
        var response = await fetch(`/api/Post/like-post/${post.author.id}/${post.id}`, {
                method: "PATCH",
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `bearer ${token}`
                }
            }
        )

        if (response.ok) {
            var userAndPost = await response.json()
            
            dispatch(setUser({ user: userAndPost.user }))
            setUPost(userAndPost.post)
        }
    }

    function getTimeSinceString(extractedPost: Post): string {
        const postDate = new Date(extractedPost.createdAt)
        const timeDifference = currentDate.getTime() - postDate.getTime()
        const seconds = timeDifference/1000
        let timeString = ""

        let interval = seconds / 31536000; 
        if (interval > 1) {
            timeString = postDate.toLocaleDateString()
            return timeString
        } 

        interval = seconds / 2592000; 
        if (interval > 1) {
            return `${Math.floor(interval)}m`
        }  

        interval = seconds / 604800; 
        if (interval > 1) {
            return `${Math.floor(interval)}w`
        } 

        interval = seconds / 86400; 
        if (interval > 1) {
            return `${Math.floor(interval)}d`
        }  

        interval = seconds / 3600; 
        if (interval > 1) {
            return `${Math.floor(interval)}h`
        }  

        interval = seconds / 60; 
        if (interval > 1) {
            return `${Math.floor(interval)}m`
        }  

        return `${Math.floor(seconds)}s`
    }

    function addToPostReplies(reply: Post) {
        post?.replies.push(reply)
    }

    function navigateToQuotedPost() {
        navigate(`/profile/${post?.quotedPost?.author.userName}/post/${post?.quotedPost?.id}`)
    }

    function getTextWithLinks() {
        let words = uPost.text.split(/\s+/)
        let urls: string[] = []
        const input = uPost.text
        let textParts: string[] = []
        const urlRegex = /https?:\/\/[^\s]+/g;
        let lastIndex = 0

        for (let word of words) {
            try {
                if (!word.includes("http")) {
                    continue
                }
                let potentialUrl = new URL(word)
                let url = potentialUrl.href
                urls.push(url)
            } catch (error) {
                
            }
        }

        input.replace(urlRegex, (url, index) => {
            if (index > lastIndex) {
                textParts.push(input.slice(lastIndex, index));
            }
            
            textParts.push(url);
            lastIndex = index + url.length;
            return url;
        }); 
        
        if (lastIndex < input.length) { 
            textParts.push(input.slice(lastIndex)); 
        }

        return (
            <div>
                {textParts.map((part) => {
                    return (
                        <>
                        {urls.includes(part)
                            ? <a href={part} target="_blank">{part}</a>
                            : <span>{part}</span>
                        }
                        </>
                    )
                })}
            </div>
        )
    }

    return (
        <div key={uPost.id} style={{display: 'flex', flexDirection: 'column', width: '100%', 
            height: 'auto', borderBottom: "1px solid cyan", padding: "1em", paddingRight: "0em"}}>
        <div  style={{ display: "flex", flexDirection: "row"  }}>
            <div>
                <img src={uPost.author.profilePicture} width={40} height={40} style={{ borderRadius: "50%" }} />
            </div>
            <div style={{ display: "flex", flexDirection: "column", flex: "1 1 0%" }}>
                <div>
                    <div style={{ display: 'flex', flexDirection: 'row', 
                                    alignItems: "center", gap: "4px" }}>
                        <span style={{ marginLeft: '1em', fontWeight: 'bold', cursor: 'pointer' }}
                            onClick={() => navigate(`/profile/${uPost.author.userName}`)}>
                            {uPost.author.displayName}
                        </span>
                        <span>
                            {" @"}{uPost.author.userName}
                        </span>
                        <span>
                            {" "}{getTimeSinceString(post)}
                        </span>
                    </div>
                    <div style={{ width: "auto" }} >
                        <div style={{ width: "auto", paddingRight: '2em' }}>
                            <div style={{ marginLeft: '1em', marginTop: '1em', width: "auto",
                                marginBottom: '1em', cursor: 'pointer'  }}>
                                <span onClick={() => navigate(`/profile/${post.author.userName}/post/${post.id}`)}>
                                    {uPost.embed.externalLink
                                    ? getTextWithLinks()
                                    : uPost.text}
                                </span>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
                
            {showModal
                ? <div>
                    <ReplyModal post={post != null ? post : undefined} setShowModal={setShowModal}
                        addToPostReplies={addToPostReplies} />
                  </div>
                : null}
            {showQuoteModal
                ? <div>
                    <QuoteModal post={post != null ? post : undefined} setShowQuoteModal={setShowQuoteModal} />
                  </div>
                : null}
        </div>
        <div style={{ display: 'flex', flexDirection: 'column', marginLeft: '2.5em' }}>
            {uPost.embed.embedType === EmbedType.Images
            ?
                uPost.embed.images?.length! < 3
                ? 
                <div style={{ display: 'flex', flexDirection: 'row', marginLeft: '1em', gap: '10px' }}>
                    {uPost.embed.images?.map((image) => {
                        return (
                            <div key={image.filePath}>
                                {uPost.embed.images?.length == 1
                                ? 
                                    <img src={image.filePath} alt={image.altText} style={{ maxWidth: '30em' }} />
                                : 
                                    <div style={{ display: 'flex', flexDirection: 'column' }}>
                                        <img src={image.filePath} alt={image.altText} style={{ maxWidth: '15em' }} />
                                    </div>
                                }
                            </div>
                        )
                    })}
                </div>
                : uPost.embed.images?.length === 3
                ? 
                <div style={{ display: 'flex', flexDirection: 'row', gap: '10px', marginLeft: '1em' }}>
                    <div style={{ display: 'flex', flexDirection: 'column' }}>
                        <img src={uPost.embed.images[0].filePath} alt={uPost.embed.images[0].altText} style={{ maxWidth: '15em' }} />
                    </div>
                    <div style={{ display: 'flex', flexDirection: 'column', gap: '10px' }}>
                        <div style={{ display: 'flex', flexDirection: 'column' }}>
                            <img src={uPost.embed.images[1].filePath} alt={uPost.embed.images[1].altText} style={{ maxHeight: '10em' }} />
                        </div>
                        <div style={{ display: 'flex', flexDirection: 'column' }}>
                            <img src={uPost.embed.images[2].filePath} alt={uPost.embed.images[2].altText} style={{ maxHeight: '10em' }} />
                        </div>
                    </div>
                </div>
                :
                <div style={{ display: 'flex', flexDirection: 'column', gap: '10px', marginLeft: '1em' }}>
                    <div style={{ display: 'flex', flexDirection: 'row', gap: '10px' }}>
                        <div style={{ display: 'flex', flexDirection: 'column' }}>
                            <img src={uPost.embed.images?.[0].filePath} alt={uPost.embed.images?.[0].altText}
                                style={{ maxWidth: '12em' }} />
                        </div>
                        <div style={{ display: 'flex', flexDirection: 'column' }}>
                            <img src={uPost.embed.images?.[1].filePath} alt={uPost.embed.images?.[1].altText}
                                style={{ maxWidth: '12em' }} />
                        </div>
                    </div>
                    <div style={{ display: 'flex', flexDirection: 'row', gap: '10px' }}>
                        <div style={{ display: 'flex', flexDirection: 'column' }}>
                            <img src={uPost.embed.images?.[2].filePath} alt={uPost.embed.images?.[2].altText}
                                style={{ maxWidth: '12em' }} />
                        </div>
                        <div style={{ display: 'flex', flexDirection: 'column' }}>
                            <img src={uPost.embed.images?.[3].filePath} alt={uPost.embed.images?.[3].altText}
                                style={{ maxWidth: '12em' }} />
                        </div>
                    </div>
                </div>
            : null
            }
            {uPost.embed.externalLink
            ?
                <div style={{ marginLeft: '1em', border: '1px solid #6B7575', borderRadius: '0.5em',
                    width: '90%' }}>
                    <div style={{ borderBottom: '1px solid #6B7575' }}>
                        <a href={uPost.embed.externalLink.externalLinkUri}
                        target="_blank">
                            <img src={uPost.embed.externalLink.externalLinkThumbnail} 
                            style={{ width: '100%', borderRadius: '0.5em' }} />
                        </a>
                    </div>
                    <div style={{  
                        borderBottom: '1px solid #6B7575', marginLeft: '0.5em', 
                        marginRight: '0.5em' }}>
                        <span style={{ width: '1em', fontWeight: 'bold' }}>
                            {uPost.embed.externalLink.externalLinkTitle}
                        </span>
                        <hr />
                        <span style={{ fontWeight: 'lighter' }}>
                            {uPost.embed.externalLink.externalLinkDescription}
                        </span>
                    </div>
                </div>
            : null
            }

            {post.quotedPost
            ? <div style={{ display: 'flex', flexDirection: 'row', border: '1px solid cyan',
                borderRadius: '0.5em', marginTop: '0.5em', marginRight: '1em',
                marginLeft: '1em' }}>
                <div style={{ display: 'flex', marginTop: '0.5em' }}>
                    <div style={{ display: 'flex', flexDirection: 'column', paddingBottom: '1em', cursor: 'pointer' }}
                        onClick={() => navigateToQuotedPost()}>
                        <div style={{ display: 'flex', flexDirection: 'row', gap: '4px' }}>
                            <img src={post.quotedPost.author.profilePicture} width={30} height={30} style={{ borderRadius: '50%', marginLeft: '0.5em' }} />
                            <span>{post?.quotedPost.author.displayName}</span>
                            <span> {" @"}{post?.quotedPost.author.userName}</span>
                            <span> {" "}{getTimeSinceString(post.quotedPost)}</span>
                        </div>
                        <span style={{ marginLeft: '0.5em' }}>{post?.quotedPost.text}</span>
                    </div>
                </div>
            </div>
            : null}
        </div>
        <div style={{ display: 'flex', flexDirection: 'row', marginLeft: '2.5em', gap: '5em' }}>
            <div style={{ display: 'flex', flexDirection: 'row', 
                marginLeft: '1em', marginTop: '1em', justifyContent: "space-between", gap: '5em' }}>
                <div style={{ flex: '1 1 0%', alignItems: 'flex-start', marginLeft: '-6px' }}>
                    <button style={{ display: 'flex', backgroundColor: '#242424', 
                        textAlign: 'center', flexDirection: 'row', justifyContent: 'center',
                        alignItems: 'center' }}
                        onClick={() => setShowModal(true)}>
                        <i style={{ fontSize: '1.3em' }} className="material-symbols-outlined">
                            chat_bubble 
                        </i>
                        <span style={{ marginLeft: '0.2em' }}>
                            {uPost.replyCount >= 1000
                            ? uPost.replyCount/1000+"k"
                            : uPost.replyCount}
                        </span>
                    </button>
                </div>

                <div style={{ flex: '1 1 0%', alignItems: 'flex-start' }}>
                    <button style={{ display: 'flex', backgroundColor: '#242424', 
                        textAlign: 'center', flexDirection: 'row', justifyContent: 'center', 
                        alignItems: 'center' }}
                        onClick={() => setShowQuoteModal(true)}>
                        <i style={{ fontSize: '1.3em' }} className="material-symbols-outlined">
                            repeat
                        </i>
                        <span style={{ marginLeft: '0.2em' }}>
                            {uPost.repostCount+uPost.quoteCount >= 1000
                            ? (uPost.repostCount+uPost.quoteCount)/1000+"k"
                            : uPost.repostCount+uPost.quoteCount}
                        </span>
                    </button>
                </div>

                <div style={{ flex: '1 1 0%', alignItems: 'flex-start' }}>
                    <button style={{ display: 'flex', backgroundColor: '#242424', 
                        textAlign: 'center', flexDirection: 'row', justifyContent: 'center', 
                        alignItems: 'center' }} onClick={likePost}>
                        
                        {user!.likedPosts.some(p => p.id === post.id)
                        ? <i style={{ fontSize: '1.3em', color: 'red' }} className="material-icons">
                            favorite
                        </i>
                        : <i style={{ fontSize: '1.3em' }} className="material-symbols-outlined">
                            favorite
                        </i>
                        }
                        
                        <span style={{ marginLeft: '0.2em' }}>
                            {uPost.likeCount >= 1000
                            ? uPost.likeCount/1000+"k"
                            : uPost.likeCount}
                        </span>
                    </button>
                </div>
            </div>
        </div>
    </div>
    )
}