import React from 'react';
import { NavLink } from "react-router-dom";
import { useDispatch } from "react-redux";
import { useSelector } from "react-redux";
import { setLogout, RootState } from "../state";
import {  User } from "../types";
import { useNavigate } from "react-router";

type LayoutProps = {
    children: React.ReactNode;
};

const Layout: React.FC<LayoutProps> = ({ children }) => {
    const user = useSelector<RootState, User | null>((state) => state.user);
    const dispatch = useDispatch();
    const navigate = useNavigate();

    function logOut() {
        dispatch(setLogout())
        navigate("/")
    }

    return (
        <div style={{ display:' flex', flexDirection: 'row', marginLeft: "15vw" }}>
            <nav style={{ display:' flex', alignItems:'center', flexDirection: 'column', 
                width: '20vw', fontSize: "1.5em", paddingTop: '1em', borderRight: "1px solid cyan" }}>
                <NavLink to={`/${user?.userName}`}>Profile</NavLink>
                <NavLink to={"/"}>Home</NavLink>
                <NavLink to={""} onClick={logOut}>Logout</NavLink>
            </nav>

            <div style={{ display:' flex', flexDirection: 'column', width: '30vw', paddingTop: '1em' }}>
                {children}
            </div>
            <div style={{ display:' flex', alignItems:'center', flexDirection: 'column', 
                        width: '20vw', paddingTop: '1em', borderLeft: "1px solid cyan" }}>

            </div>
        </div>
    );
};

export default Layout;
