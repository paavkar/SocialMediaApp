import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import SignIn from './Pages/SignIn'
import Register from './Pages/Register';
import { HomePage } from './Pages/HomePage';
import { RootState } from "./state";
import { useSelector } from "react-redux";
import { ProfilePage } from './Pages/ProfilePage';

function App() {
    const isAuth = Boolean(useSelector<RootState>((state) => state.token));

    return (
        <BrowserRouter>
            <Routes>
                <Route path='/' element={isAuth ? <HomePage /> : <SignIn /> } />
                <Route path='/login' element={isAuth ? <Navigate to="/" /> : <SignIn /> } />
                <Route path='/register' element={isAuth ? <Navigate to="/" /> : <Register /> } />
                <Route path='/:userName' element={<ProfilePage />} />
                <Route path="*" element={<Navigate to="/" replace />} />
            </Routes>
        </BrowserRouter>
    );
}

export default App;