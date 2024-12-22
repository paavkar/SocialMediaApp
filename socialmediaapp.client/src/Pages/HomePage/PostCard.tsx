import { Post, User } from "../../types"
import { RootState } from "../../state";
import { useSelector } from "react-redux";
import { useState } from "react";
import { useNavigate } from "react-router";
import { useDispatch } from "react-redux";
import { setUser } from "../../state";

type PostProps = {
    post: Post;
}

export function PostCard({ post }: PostProps) {
    const [fetchedPost, setFetchedPost] = useState<Post>(post)
    const [currentDate, setCurrentDate] = useState<Date>(new Date())
    const token = useSelector<RootState, string | null>((state) => state.token);
    const user = useSelector<RootState, User | null>((state) => state.user);
    
    const dispatch = useDispatch();
    const navigate = useNavigate();

    async function fetchPost() {
        var response = await fetch(`api/Post/post/${post.author.userName}/${post.id}`, {
                method: "GET",
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `bearer ${token}`
                }
            }
        )
        if (response.ok) {
            var postJson = await response.json()
            setFetchedPost(postJson)
        }
    }
    
    async function likePost() {
        var response = await fetch(`api/Post/like-post/${post.author.id}/${post.id}`, {
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
            setFetchedPost(userAndPost.post)
        }
    }

    return (
        <div style={{  }}>
            <div>
                <div key={fetchedPost.id} style={{ display: 'flex', flexDirection: 'column', 
                                                    justifyContent: 'flex-start', 
                                                    width: '100%', 
                                                    height: 'auto' }}>
                    <div style={{ cursor: 'pointer'}} onClick={() => navigate(`${post.author.userName}`)}>
                        <span style={{ marginLeft: '1em', fontWeight: 'bold' }}>
                            {fetchedPost.author.displayName}
                        </span>
                        <span>
                            {" @"}{fetchedPost.author.userName}
                        </span>
                    </div>
                    <div style={{ marginLeft: '1em', marginTop: '1em' }}>
                        <span>{fetchedPost.text}</span>
                    </div>
                </div>
                <div style={{ display: 'flex', flexDirection: 'row', justifyContent: 'space-between', 
                        marginLeft: '1em', marginTop: '1em' }}>

                    <div style={{ flex: '1 1 0%', alignItems: 'flex-start', marginLeft: '-6px' }}>
                        <button style={{ display: 'flex', backgroundColor: '#242424', 
                            textAlign: 'center', flexDirection: 'row', justifyContent: 'center',
                            alignItems: 'center'
                                }}>
                            <i style={{ fontSize: '1.3em' }} className="material-symbols-outlined">
                                chat_bubble 
                            </i>
                            <span style={{ marginLeft: '0.2em' }}> {fetchedPost.replyCount} </span>
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
                            <span style={{ marginLeft: '0.2em' }}> {fetchedPost.repostCount} </span>
                        </button>
                    </div>

                    <div style={{ flex: '1 1 0%', alignItems: 'flex-start' }}>
                        <button style={{ display: 'flex', backgroundColor: '#242424', 
                            textAlign: 'center', flexDirection: 'row', justifyContent: 'center', 
                            alignItems: 'center' }} onClick={likePost}>
                            
                            {user?.likedPosts.some(p => p.id === fetchedPost.id)
                            ? <i style={{ fontSize: '1.3em', color: 'red' }} className="material-icons">
                                favorite
                              </i>
                            : <i style={{ fontSize: '1.3em' }} className="material-symbols-outlined">
                                favorite
                              </i>
                            }
                            
                            <span style={{ marginLeft: '0.2em' }}> {fetchedPost.likeCount} </span>
                        </button>
                    </div>
                    
                </div>
                <hr style={{ borderColor: "cyan" }} />
            </div>
        </div>
    )
}