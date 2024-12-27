import { Post, User } from "../../types"
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
            Math.floor(interval) > 1
            ? timeString = `${Math.floor(interval)} months ago`
            : timeString = `${Math.floor(interval)} month ago`
            return timeString
        } 

        interval = seconds / 604800; 
        if (interval > 1) {
            Math.floor(interval) > 1
            ? timeString = `${Math.floor(interval)} weeks ago`
            : timeString = `${Math.floor(interval)} week ago`
            return timeString
        } 

        interval = seconds / 86400; 
        if (interval > 1) { 
            Math.floor(interval) > 1
            ? timeString = `${Math.floor(interval)} days ago`
            : timeString = `${Math.floor(interval)} day ago`
            return timeString
        } 

        interval = seconds / 3600; 
        if (interval > 1) {
            Math.floor(interval) > 1
            ? timeString = `${Math.floor(interval)} hours ago`
            : timeString = `${Math.floor(interval)} hour ago`
            return timeString
        } 

        interval = seconds / 60; 
        if (interval > 1) { 
            Math.floor(interval) > 1
            ? timeString = `${Math.floor(interval)} minutes ago`
            : timeString = `${Math.floor(interval)} minute ago`
            return timeString
        } 

        return `${Math.floor(seconds)} seconds ago`
    }

    function addToPostReplies(reply: Post) {
        post?.replies.push(reply)
    }

    function navigateToQuotedPost() {
        navigate(`/profile/${post?.quotedPost?.author.userName}/post/${post?.quotedPost?.id}`)
    }

    return (
        <div key={uPost.id} style={{ display: "flex", flexDirection: "row", width: '100%', 
            height: 'auto', borderBottom: "1px solid cyan", padding: "1em", paddingRight: "0em",
            flex: "1 1 0%" }}>
            <div>
                <img src="" width={40} height={40} style={{ borderRadius: "50%" }} />
            </div>
            <div style={{ display: "flex", flexDirection: "column", flex: "1 1 0%" }}>
                <div>
                    <div style={{ display: 'flex', flexDirection: 'row', 
                                    alignItems: "center", gap: "4px", flex: "1 1 0%" }}>
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
                    <div style={{ flex: "1 1 0%", width: "auto", cursor: 'pointer' }} 
                        onClick={() => navigate(`/profile/${uPost.author.userName}/post/${uPost.id}`)}>
                        <div style={{ flex: "1 1 0%", width: "auto" }}>
                            <div style={{ marginLeft: '1em', marginTop: '1em', flex: "1 1 0%", width: "auto" }}>
                                {uPost.text}</div>
                            </div>
                    </div>
                    
                    {post.quotedPost
                    ? <div style={{ display: 'flex', flexDirection: 'row', border: '1px solid cyan',
                        borderRadius: '0.5em', marginTop: '0.5em', marginRight: '1em',
                        marginLeft: '1em' }}>
                        <div style={{ display: 'flex', marginTop: '0.5em' }}>
                            <div style={{ display: 'flex', flexDirection: 'column', paddingBottom: '1em', cursor: 'pointer' }}
                                onClick={() => navigateToQuotedPost()}>
                                <div style={{ display: 'flex', flexDirection: 'row', gap: '4px' }}>
                                    <img src="" width={20} height={20} style={{ borderRadius: '50%', margin: '0.5em' }} />
                                    <span>{post?.quotedPost.author.displayName}</span>
                                    <span> {" @"}{post?.quotedPost.author.userName}</span>
                                    <span> {" "}{getTimeSinceString(post.quotedPost)}</span>
                                </div>
                                <span style={{ marginLeft: '0.5em' }}>{post?.quotedPost.text}</span>
                            </div>
                        </div>
                    </div>
                    : null}
                    
                    
                    <div style={{ display: 'flex', flexDirection: 'row', 
                        marginLeft: '1em', marginTop: '1em', justifyContent: "space-between" }}>

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
    )
}