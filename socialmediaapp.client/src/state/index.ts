import { createSlice } from "@reduxjs/toolkit";
import { User } from "../types";

type InitialState = {
    user: User | null,
    token: string | null
}

const initialState: InitialState = {
    user: null,
    token: null
}

export const authSlice = createSlice({
    name: "auth",
    initialState,
    reducers: {
        setLogin: (state, action) => {
            state.user = action.payload.user,
            state.token = action.payload.token
        },
        setLogout: (state) => {
            state.user = null,
            state.token = null
        },
        setUser: (state, action) => {
            state.user = action.payload.user
        }
    }
})

export type RootState = ReturnType<typeof authSlice.reducer>

export const { setLogin, setLogout, setUser } = authSlice.actions;
export default authSlice.reducer;