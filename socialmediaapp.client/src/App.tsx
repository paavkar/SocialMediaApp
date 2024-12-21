import { BrowserRouter, Routes, Route, Navigate } from 'react-router';
import SignIn from './Pages/SignIn'
import Register from './Pages/Register';
import { HomePage } from './Pages/HomePage';
import { User } from './types';
import { RootState } from "./state";
import { useDispatch } from "react-redux";
import { useSelector } from "react-redux";

function App() {
    const isAuth = Boolean(useSelector<RootState>((state) => state.token));
    const user = useSelector<RootState, User | null>((state) => state.user);
    const token = useSelector<RootState, string | null>((state) => state.token);
    const dispatch = useDispatch();

    return (
        <div>
            <BrowserRouter>
            <Routes>
                <Route path='/' element={isAuth ? <HomePage /> : <SignIn /> } />
                <Route path='/login' element={isAuth ? <Navigate to="/" /> : <SignIn /> } />
                <Route path='/register' element={isAuth ? <Navigate to="/" /> : <Register /> } />
                <Route path="*" element={<Navigate to="/" replace />} />
            </Routes>
            </BrowserRouter>
        </div>
    );
}

export default App;