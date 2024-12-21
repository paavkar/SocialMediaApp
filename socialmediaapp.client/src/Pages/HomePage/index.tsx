import { useEffect, useState } from "react";
import { Post } from "../../types";
import { RootState } from "../../state";
import { useSelector } from "react-redux";
import { PostCard } from "./PostCard";

export function HomePage() {
    const [posts, setPosts] = useState<Post[]>([]);
    const token = useSelector<RootState, string | null>((state) => state.token);

    async function fetchPosts() {
        var response = await fetch("api/Post", {
            method: "GET",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `bearer ${token}`
            }
        })

        if (response.status === 401) {
            console.log(response.statusText)
        }

        if (response.ok) {
            var jsonPosts: Post[] = await response.json()

            setPosts(jsonPosts)
        }
    }

    useEffect(() => {
        fetchPosts();
        console.log(posts.length)
        console.log(posts)
    }, [])

    return (
        <div style={{ display:' flex', alignItems:'center', flexDirection: 'column', width: '90vw', marginLeft: '5em' }}>
            <h1>Posts</h1>

            <div>
                {posts.map((post) => {
                    return (
                        <PostCard post={post} />
                    )
                })}
            </div>
        </div>
    )
}