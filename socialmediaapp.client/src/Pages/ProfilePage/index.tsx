import { useParams, NavLink } from "react-router"
import { useSelector, useDispatch } from "react-redux";
import { RootState, setLogout } from "../../state";
import { useEffect, useState } from "react";
import { User } from "../../types";
import Layout from "../layout";

export function ProfilePage() {
    const { userName } = useParams()
    const dispatch = useDispatch()
    const token = useSelector<RootState, string | null>((state) => state.token);
    const [user, setUser] = useState<User>()

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

    useEffect(() => {
        fetchUser()
    }, [])

    return (
        <Layout>
            <div>
                {user?.displayName}
            </div> 
        </Layout>
    )
}