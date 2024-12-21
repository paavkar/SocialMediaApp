import React from 'react';
import { NavLink } from "react-router";
import { useDispatch } from "react-redux";
import { useSelector } from "react-redux";
import { setLogout, RootState } from "../state";
import {  User } from "../types";

type LayoutProps = {
    children: React.ReactNode;
};

const Layout: React.FC<LayoutProps> = ({ children }) => {
    const user = useSelector<RootState, User | null>((state) => state.user);
    const dispatch = useDispatch();
    return (
        <div style={{ display:' flex', flexDirection: 'row', width: '90vw', marginLeft: '5em' }}>
            <div style={{ display:' flex', alignItems:'center', flexDirection: 'column', 
                width: '30vw', fontSize: "1.5em", marginTop: '1em' }}>
                <NavLink to={`/${user?.userName}`}>Profile</NavLink>
                <NavLink to={"/"}>Home</NavLink>
                <NavLink to={""} onClick={() => dispatch(setLogout())}>Logout</NavLink>
            </div>
            <div style={{ borderLeft: '1px solid cyan', height: '100vw' }}></div>

            <div style={{ display:' flex', flexDirection: 'column', width: '30vw', marginTop: '1em' }}>
                {children}
            </div>
            
            <div style={{ borderLeft: '1px solid cyan', height: '100vw' }}></div>
            <div style={{ display:' flex', alignItems:'center', flexDirection: 'column', 
                        width: '30vw', marginTop: '1em' }}>

            </div>
        </div>
    );
};

export default Layout;
