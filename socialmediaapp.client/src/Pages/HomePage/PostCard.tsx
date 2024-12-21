import { Post } from "../../types"
import { RootState } from "../../state";
import { useSelector } from "react-redux";
import { useEffect, useState } from "react";
type PostProps = {
    post: Post;
}

export function PostCard({ post }: PostProps) {
    const [fetchedPost, setFetchedPost] = useState<Post>(post)
    const token = useSelector<RootState, string | null>((state) => state.token);

    async function fetchPost() {
        var response = await fetch(`api/Post/${post.author.userName}/${post.id}`, {
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
        fetchPost()
    }, [])


    return (
        
        <div key={fetchedPost.id}>
            <span>{fetchedPost.author.displayName}</span>
            <span>{" "}{fetchedPost.author.userName}</span>
            <div>
                <span>{fetchedPost.text}</span>
            </div>
        </div>
        
    )
}