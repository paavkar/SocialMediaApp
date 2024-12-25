import { useEffect, useState } from "react";
import { Post } from "../../types";
import { RootState } from "../../state";
import { useSelector } from "react-redux";
import { PostCard } from "./PostCard";
import { NewPost } from "./NewPost";
import Layout from "../layout"

export const HomePage = () => {
    const [posts, setPosts] = useState<Post[]>([]);
    const token = useSelector<RootState, string | null>((state) => state.token);

    async function fetchPosts() {
        var response = await fetch("/api/Post", {
            method: "GET",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `bearer ${token}`
            }
        })

        if (response.ok) {
            var jsonPosts: Post[] = await response.json()

            setPosts(jsonPosts)
        }
    }

    useEffect(() => {
        fetchPosts();
    }, [])


    return (
        <Layout>
            <div>
                <NewPost />
            </div>
            <div style={{ marginRight: "1em" }}>
                {posts.map((post) => {
                    return (
                        <PostCard post={post} />
                    )
                })}
            </div> 
        </Layout>
    )
}