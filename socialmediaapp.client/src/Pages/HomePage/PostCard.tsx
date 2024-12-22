import { Post } from "../../types"
import { RootState } from "../../state";
import { useSelector } from "react-redux";
import { useEffect, useState } from "react";
import { useNavigate } from "react-router";

type PostProps = {
    post: Post;
}

export function PostCard({ post }: PostProps) {
    const [fetchedPost, setFetchedPost] = useState<Post>(post)
    const [currentDate, setCurrentDate] = useState<Date>(new Date())
    const token = useSelector<RootState, string | null>((state) => state.token);
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

    useEffect(() => {
        //fetchPost()
    }, [])


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
                            alignItems: 'center'
                                }}>
                            <i style={{ fontSize: '1.3em' }} className="material-symbols-outlined">
                                favorite
                            </i>
                            <span style={{ marginLeft: '0.2em' }}> {fetchedPost.likeCount} </span>
                        </button>
                    </div>
                    
                </div>
                <hr style={{ borderColor: "cyan" }} />
            </div>
        </div>
    )
}