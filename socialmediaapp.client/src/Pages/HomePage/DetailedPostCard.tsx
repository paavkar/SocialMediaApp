import { Post, User } from "../../types"
import { RootState } from "../../state";
import { useSelector } from "react-redux";
import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useDispatch } from "react-redux";
import { setUser } from "../../state";
import Layout from "../layout";
import { PostCard } from "./PostCard";
import { NewPostModal } from "./NewPostModal";

export const DetailedPostCard = () => {
    const { userName } = useParams()
    const { postId } = useParams()
    const [post, setPost] = useState<Post | null>(null)
    const [showModal, setShowModal] = useState(false)
    const token = useSelector<RootState, string | null>((state) => state.token);
    const user = useSelector<RootState, User | null>((state) => state.user);
    
    const dispatch = useDispatch();
    const navigate = useNavigate();
    
    async function likePost() {
        var response = await fetch(`/api/Post/like-post/${post?.author.id}/${post?.id}`, {
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
            setPost(userAndPost.post)
        }
    }

    async function fetchPost() {
        var response = await fetch(`/api/Post/post/${userName}/${postId}`, {
                method: "GET",
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `bearer ${token}`
                }
            }
        )
        
        if (response.ok) {
            var postJson = await response.json()
            setPost(postJson)
        }
    }

    function getTimeString() {
        const postDate = new Date(post!.createdAt)

        return `${postDate.toDateString()} at ${postDate.toLocaleTimeString()}`
    }

    function addToPostReplies(reply: Post) {
        post?.replies.push(reply)
    }

    useEffect(() => {
        {post === null
            ? fetchPost()
            : null
        }
    })

    return (
        <Layout>
            <div>
                <div style={{ marginRight: '1em', display: "flex", flexDirection: "row", width: '100%', 
                        height: 'auto',
                        flex: "1 1 0%" }}>
                    {post === null
                    ? <div>Post not found</div>
                    : <div key={post!.id} style={{ display: "flex", flexDirection: "row", width: '100%', 
                        height: 'auto', borderBottom: "1px solid cyan", padding: "1em", paddingRight: "0em",
                        flex: "1 1 0%" }}>
                        <div style={{ display: "flex", flexDirection: "row", flex: "1 1 0%" }}>
                            <div style={{ display: 'flex', flexDirection: 'column', flex: '1 1 0%' }}>
                                <div style={{ display: 'flex', flexDirection: 'row', 
                                                alignItems: "center", gap: "4px", flex: "1 1 0%" }}>
                                    
                                    <img src="" width={40} height={40} style={{ borderRadius: "50%" }} />
                                    <div style={{ display: 'flex', flexDirection: 'column' }}>
                                        <span style={{ marginLeft: '1em', fontWeight: 'bold', cursor: 'pointer' }}
                                        onClick={() => navigate(`/profile/${post!.author.userName}`)}>
                                        {post!.author.displayName}
                                    </span>
                                    <span style={{ marginLeft: '1em' }}>
                                        {"@"}{post!.author.userName}
                                    </span>
                                    </div>
                                </div>
                                <div style={{ flex: "1 1 0%", width: "auto" }}>
                                    <div style={{ flex: "1 1 0%", width: "auto" }}>
                                    <div style={{ marginTop: '1em', flex: "1 1 0%", width: "auto" }}>
                                        {post!.text}
                                    </div>
                                    </div>
                                </div>

                                <div style={{ borderTop: '1px solid cyan',
                                    borderBottom: '1px solid cyan', marginRight: '1em', marginTop: '1em',
                                    paddingTop: '0.5em', paddingBottom: '0.5em', fontSize: '0.9em' }}>
                                    <span>{getTimeString()}</span>
                                </div>
                                
                                {(post.accountsLiked.length > 0
                                    || post.quotes.length > 0
                                    || post.accountsReposted.length > 0
                                ? <div style={{ display: 'flex', flexDirection: 'row', 
                                    justifyContent: 'space-between',
                                    flex: '1 1 0%', borderBottom: '1px solid cyan', marginRight: '1em' }}>

                                    {post?.accountsReposted?.length > 0
                                    ? <div style={{ display: 'flex', flex: '1 1 0%',
                                        marginRight: '1em', marginTop: '1em',
                                        paddingBottom: '0.5em', fontSize: '0.9em' }}>
                                            {post?.accountsReposted?.length == 1
                                            ?  <span>{post?.accountsReposted?.length} repost</span>
                                            : <span>{post?.accountsReposted?.length} reposts</span> }
                                        </div>
                                    : null }
                                    
                                    {post?.quotes?.length > 0
                                    ? <div style={{ display: 'flex', flex: '1 1 0%',
                                        marginRight: '1em', marginTop: '1em',
                                        paddingBottom: '0.5em', fontSize: '0.9em' }}>
                                            {post?.quotes?.length == 1
                                                ? <span>{post?.quotes?.length} quote</span>
                                                : <span>{post?.quotes?.length} quotes</span> }
                                        </div>
                                    : null }
                                    
                                    {post?.accountsLiked?.length > 0
                                    ? <div style={{ display: 'flex', flex: '1 1 0%',
                                        marginRight: '1em', marginTop: '1em',
                                        paddingBottom: '0.5em', fontSize: '0.9em' }}>
                                            {post?.accountsLiked?.length == 1
                                                ?  <span>{post?.accountsLiked?.length} like</span>
                                                : <span>{post?.accountsLiked?.length} likes</span> }
                                        </div>
                                    : null }
                                </div>
                                : null)}
                                
                                
                                <div style={{ display: 'flex', flexDirection: 'row', 
                                    marginTop: '1em', justifyContent: "space-between" }}>

                                    <div style={{ flex: '1 1 0%', alignItems: 'flex-start'}}>
                                        <button style={{ display: 'flex', backgroundColor: '#242424', 
                                            textAlign: 'center', flexDirection: 'row', justifyContent: 'center',
                                            alignItems: 'center' }}
                                            onClick={() => setShowModal(true)}>
                                            <i style={{ fontSize: '1.3em' }} className="material-symbols-outlined">
                                                chat_bubble 
                                            </i>
                                            <span style={{ marginLeft: '0.2em' }}> {post!.replyCount} </span>
                                        </button>
                                    </div>

                                    <div style={{ flex: '1 1 0%', alignItems: 'flex-start' }}>
                                        <button style={{ display: 'flex', backgroundColor: '#242424', 
                                            textAlign: 'center', flexDirection: 'row', justifyContent: 'center', 
                                            alignItems: 'center'
                                                }}>
                                            <i style={{ fontSize: '1.3em' }} className="material-symbols-outlined">
                                                repeat
                                            </i>
                                            <span style={{ marginLeft: '0.2em' }}> {post!.repostCount} </span>
                                        </button>
                                    </div>

                                    <div style={{ flex: '1 1 0%', alignItems: 'flex-start' }}>
                                        <button style={{ display: 'flex', backgroundColor: '#242424', 
                                            textAlign: 'center', flexDirection: 'row', justifyContent: 'center', 
                                            alignItems: 'center' }} onClick={likePost}>
                                            
                                            {user!.likedPosts.some(p => p.id === post!.id)
                                            ? <i style={{ fontSize: '1.3em', color: 'red' }} className="material-icons">
                                                favorite
                                            </i>
                                            : <i style={{ fontSize: '1.3em' }} className="material-symbols-outlined">
                                                favorite
                                            </i>
                                            }
                                            
                                            <span style={{ marginLeft: '0.2em' }}> {post!.likeCount} </span>
                                        </button>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div> }
                </div>
                {showModal
                ? <div>
                    <NewPostModal post={post != null ? post : undefined} setShowModal={setShowModal}
                        addToPostReplies={addToPostReplies} />
                  </div>
                : null}

                <div style={{ borderBottom: '1px solid cyan', cursor: 'pointer' }}
                    onClick={() => setShowModal(true)}>
                    <div style={{ display: 'flex', flexDirection: 'row' }} onClick={() => setShowModal(true)}>
                        <img src="" width={40} height={40} style={{ borderRadius: '50%', margin: '0.5em' }} />
                        <button style={{ display: 'flex', justifyContent: 'flex-start', width: '100%',
                            color: '#6B7575',
                            backgroundColor: '#242424', border: 'none', resize: 'none',
                            height: '3em', fontSize: '17px', outline: 'none', marginTop: '0.5em',
                            cursor: 'pointer' }}
                            onClick={() => setShowModal(true)}>Write your reply</button>
                    </div>
                </div>

                <div style={{ marginRight: '1em' }}>
                    {post?.replies?.map((reply) => {
                        return <PostCard post={reply} />
                    })}
                    
                </div>
            </div>
    </Layout>
    )
}