import { useEffect, useState } from "react";
import { Post } from "../../types";
import { RootState } from "../../state";
import { useSelector } from "react-redux";
import { PostCard } from "./PostCard";
import { NavLink } from "react-router";
import { useDispatch } from "react-redux";
import { setLogout } from "../../state";
import { NewPost } from "./NewPost";

export function HomePage() {
    const [posts, setPosts] = useState<Post[]>([]);
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
        <div style={{ display:' flex', flexDirection: 'row', width: '90vw', marginLeft: '5em' }}>
            <div style={{ display:' flex', alignItems:'center', flexDirection: 'column', 
                width: '30vw', fontSize: "1.5em", marginTop: '1em' }}>
                <NavLink to={"/"}>Home</NavLink>
                <NavLink to={""} onClick={() => dispatch(setLogout())}>Logout</NavLink>
            </div>
            <div style={{ borderLeft: '1px solid cyan', height: '100vw' }}></div>
            <div style={{ display:' flex', flexDirection: 'column', width: '30vw', marginTop: '1em' }}>
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
            </div>
            <div style={{ borderLeft: '1px solid cyan', height: '100vw' }}></div>
            <div style={{ display:' flex', alignItems:'center', flexDirection: 'column', 
                        width: '30vw', marginTop: '1em' }}>

            </div>
        </div>
    )
}