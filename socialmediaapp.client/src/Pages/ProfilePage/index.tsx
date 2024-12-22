import { useParams, NavLink } from "react-router"
import { useSelector, useDispatch } from "react-redux";
import { RootState, setLogout } from "../../state";
import { useEffect, useState } from "react";
import { User, Post } from "../../types";
import Layout from "../layout";
import { PostCard } from "../HomePage/PostCard";

export function ProfilePage() {
    const { userName } = useParams()
    const dispatch = useDispatch()
    const token = useSelector<RootState, string | null>((state) => state.token);
    const loggedInUser = useSelector<RootState, User | null>((state) => state.user);
    const [user, setUser] = useState<User>()
    const [userPosts, setUserPosts] = useState<Post[]>()
    const [activeTab, setActiveTab] = useState("")

    async function fetchUser() {
        var response = await fetch(`api/User/${userName}`, {
                method: "GET",
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `bearer ${token}`
                }
            }
        )

        if (response.ok) {
            var userJson: User = await response.json()

            setUser(userJson)
        }
    }

    async function fetchUserPosts() {
        var response = await fetch(`api/Post/user-posts/${userName}`, {
                method: "GET",
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `bearer ${token}`
                }
            }
        )

        if (response.ok) {
            var userPostsJson: Post[] = await response.json()

            setUserPosts(userPostsJson)
        }
    }

    useEffect(() => {
        fetchUser()
        fetchUserPosts()
        setActiveTab("posts")
    }, [])

    return (
        <Layout>
            <div style={{ marginTop: '10em' }}>
                <div style={{ display: 'flex', flexDirection: 'column', marginLeft: '1em' }}>
                    <img src="" width={80} height={80} style={{ borderRadius: '1em'}} 
                        title="Profile picture if the application supported them" />
                    <div style={{ display: 'flex', flexDirection: 'row', 
                        justifyContent: 'space-between'}}>
                        <span style={{ marginTop: '0.5em', 
                            fontWeight: 'bold', fontSize: '2em' }}>
                            {user?.displayName}
                        </span>

                        {loggedInUser?.userName == user?.userName
                            ? <button style={{ height: '2em', width: '6em', marginRight: '1em', 
                                        backgroundColor: 'green'}}>
                                    Edit profile
                                </button>
                            : <button style={{ height: '2.5em', width: '5em', marginRight: '1em', 
                                backgroundColor: 'green' }}>
                                    Follow
                                </button>
                        }
                    </div>

                    <div style={{ display: 'flex', flexDirection: 'row' }}>
                        <span style={{ fontWeight: 'bold', fontSize: '1.1em' }}>
                            {user?.followers.length}
                        </span>
                        <span style={{ marginLeft: '0.2em', fontSize: '1.1em' }}>followers</span>
                        <span style={{ marginLeft: '0.4em', fontWeight: 'bold', fontSize: '1.1em' }}>
                            {user?.following.length}
                        </span>
                        <span style={{ marginLeft: '0.2em', fontSize: '1.1em' }}>following</span>
                        <span style={{ marginLeft: '0.4em', fontWeight: 'bold', fontSize: '1.1em' }}>
                            {userPosts?.length}
                        </span>
                        <span style={{ marginLeft: '0.2em', fontSize: '1.1em' }}>posts</span>
                    </div>

                    <div style={{ marginTop: '1em' }}>
                        {user?.description}
                    </div>
                </div>
                
                <div style={{ position: 'sticky', top: 0, backgroundColor: '#242424',
                        width: '100%' }}>
                    <div style={{ display: 'flex', flexDirection: 'row', 
                            justifyContent: "space-evenly", marginTop: '1em',
                        }}>
                        <div>
                            <span onClick={() => setActiveTab("posts")} 
                            style={{ fontSize: '1.2em', cursor: 'pointer' }}>
                                Posts
                            </span>
                            {activeTab == "posts"
                            ? <hr style={{ borderColor: 'cyan', width: '1.8em', 
                                margin: '0em 0em 0em 0.5em' }} />
                            : null}
                        </div>
                        
                        <div>
                            <span onClick={() => setActiveTab("likes")} 
                                style={{ fontSize: '1.2em', cursor: 'pointer' }}>
                                Likes
                            </span>
                            {activeTab == "likes"
                            ? <hr style={{ borderColor: 'cyan', width: '1.8em', 
                                margin: '0em 0em 0em 0.5em' }} />
                            : null}
                        </div>
                    </div>
                    <hr style={{ borderColor: "cyan", width: '100%' }} />
                </div>
                
                {activeTab == "posts"
                ? <div>
                    {userPosts?.map((post) => {
                        return (
                            <PostCard post={post} />
                        )
                    })}
                    </div>
                : activeTab == "likes"
                ? <div>
                    {user?.likedPosts.map((post) => {
                        return (
                            <PostCard post={post} />
                        )
                    })}
                    </div>
                : null
                }
                <div style={{ display:'flex', justifyContent: 'center' }}>
                    <span style={{ fontSize: '0.9em' }}>End of feed</span>
                </div>
            </div> 
        </Layout>
    )
}