import { Post } from "../../types"
import { RootState } from "../../state";
import { useSelector } from "react-redux";
import { useEffect, useState } from "react";

type PostProps = {
    post: Post;
}

export function PostCard({ post }: PostProps) {
    const [fetchedPost, setFetchedPost] = useState<Post>(post)
    const [count, setCount] = useState<number>(0)
    const [currentDate, setCurrentDate] = useState<Date>(new Date())
    const token = useSelector<RootState, string | null>((state) => state.token);

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
        if (count < 1) {
            fetchPost()
            setCount(count + 1)
        }
    }, [])


    return (
        <div style={{  }}>
            <div>
                <hr style={{ borderColor: "cyan" }} />
                <div key={fetchedPost.id} style={{ display: 'flex', flexDirection: 'column', 
                                                    justifyContent: 'flex-start', 
                                                    width: '100%', 
                                                    height: 'auto' }}>
                    <div>
                        <span style={{ marginLeft: '1em' }}>{fetchedPost.author.displayName}</span>
                        <span>{" @"}{fetchedPost.author.userName}</span>
                    </div>
                    <div style={{ marginLeft: '1em', marginTop: '1em' }}>
                        <span>{fetchedPost.text}</span>
                    </div>
                </div>
                <div style={{ display: 'flex', justifyContent: 'space-evenly', 
                        marginLeft: '1em', marginBottom: '1em' }}>
                    <span style={{  }}>{fetchedPost.replyCount}</span>
                    <span style={{  }}>{fetchedPost.repostCount}</span>
                    <span style={{  }}>{fetchedPost.likeCount}</span>
                </div>
                <hr style={{ borderColor: "cyan" }} />
            </div>
        </div>
    )
}