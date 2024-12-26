import { useParams } from "react-router-dom"
import { useSelector, useDispatch } from "react-redux";
import { RootState, setUser } from "../../state";
import { useEffect, useState } from "react";
import { User, Post, Author } from "../../types";
import Layout from "../layout";
import { PostCard } from "../HomePage/PostCard";
import { useNavigate } from "react-router";

export const ProfilePage = () => {
    const { userName } = useParams()
    const dispatch = useDispatch()
    const navigate = useNavigate()
    const token = useSelector<RootState, string | null>((state) => state.token);
    const loggedInUser = useSelector<RootState, User | null>((state) => state.user);
    const [user, setDisplayedUser] = useState<User>()
    const [userPosts, setUserPosts] = useState<Post[]>()
    const [activeTab, setActiveTab] = useState("")
    const isAuth = Boolean(useSelector<RootState>((state) => state.token));

    async function fetchUser() {
        var response = await fetch(`/api/User/${userName}`, {
                method: "GET",
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `bearer ${token}`
                }
            }
        )

        if (response.ok) {
            var userJson: User = await response.json()

            setDisplayedUser(userJson)
        }
    }

    async function fetchUserPosts() {
        var response = await fetch(`/api/Post/user-posts/${userName}`, {
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

    async function followUser() {
        var follower: Author = {
            id: loggedInUser!.id,
            displayName: loggedInUser!.displayName,
            description: loggedInUser!.description,
            userName: loggedInUser!.userName
        }
        var response = await fetch(`/api/User/follow-user/${userName}`, {
                method: "PATCH",
                body: JSON.stringify(follower),
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `bearer ${token}`
                }   
            }
        )

        if (response.ok) {
            var userJson = await response.json()
            
            dispatch(setUser({ user: userJson.updatedUser.user }))
            setDisplayedUser(userJson.updatedUser.followee)
        }
    }

    useEffect(() => {
        if (isAuth) {
            fetchUser()
            fetchUserPosts()
            setActiveTab("posts")
        }
        else {
            navigate("/")
        }
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
                            : <button style={{ height: '2.5em', width: '5.5em', marginRight: '1em', 
                                backgroundColor: 'green' }} onClick={followUser}>
                                    {loggedInUser!.following!.some(u => u.userName === userName)
                                    ? <span>Following</span>
                                    : loggedInUser?.followers?.some(u => u.userName === userName)
                                        ? <span>Follow back</span>
                                        : <span>Follow</span>
                                    }
                                </button>
                        }
                    </div>
                    
                    {loggedInUser?.followers?.some(u => u.userName === userName)
                    ? <div style={{ backgroundColor: "#2424", borderRadius: '0.5em', padding: '0.1em',
                        width: '5.5em'
                     }}>
                        <span style={{ fontSize: '0.9em', marginLeft: '0.5em' }}>Follows you</span>
                    </div>
                    : null}

                    <div style={{ display: 'flex', flexDirection: 'row' }}>
                        <span style={{ fontWeight: 'bold', fontSize: '1.1em' }}>
                            {user?.followers!.length}
                        </span>
                        <span style={{ marginLeft: '0.2em', fontSize: '1.1em' }}>followers</span>
                        <span style={{ marginLeft: '0.4em', fontWeight: 'bold', fontSize: '1.1em' }}>
                            {user?.following!.length}
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
                        width: '100%', borderBottom: "1px solid cyan",
                        borderRight: '1px solid cyan' }}>
                    <div style={{ display: 'flex', flexDirection: 'row', 
                            justifyContent: "space-evenly", marginTop: '1em'
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
                </div>
                
                {activeTab == "posts"
                ? <div style={{ marginRight: "1em"}}>
                    {userPosts?.map((post) => {
                        return (
                            <PostCard post={post} />
                        )
                    })}
                    
                    <div style={{ display:'flex', justifyContent: 'center', marginTop: "1em" }}>
                        <span style={{ fontSize: '0.9em' }}>End of feed</span>
                    </div>
                    </div>
                : activeTab == "likes"
                ? <div style={{ marginRight: "1em"}}>
                    
                    {user!.likedPosts.length === 0
                    ? <span style={{ display:'flex', justifyContent: 'center', marginTop: "1em" }}>
                        {user!.displayName} has no likes
                        </span>
                    : user!.likedPosts.map((post) => {
                        return (
                            <PostCard post={post} />
                        )
                    })}
                    </div>
                : null
                }
            </div> 
        </Layout>
    )
}