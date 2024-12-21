import { useEffect, useState } from "react";
import { Post, User } from "../../types";
import { RootState } from "../../state";
import { useSelector } from "react-redux";
import { PostCard } from "./PostCard";
import { NavLink } from "react-router";
import { useDispatch } from "react-redux";
import { setLogout } from "../../state";
import { NewPost } from "./NewPost";
import Layout from "../layout"

export function HomePage() {
    const [posts, setPosts] = useState<Post[]>([]);
    const user = useSelector<RootState, User | null>((state) => state.user);
    const token = useSelector<RootState, string | null>((state) => state.token);
    const dispatch = useDispatch();

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
    }, [])


    return (
        <Layout>
            <div>
                <NewPost />
            </div>
            <div>
                {posts.map((post) => {
                    return (
                        <PostCard post={post} />
                    )
                })}
            </div> 
        </Layout>
    )
}