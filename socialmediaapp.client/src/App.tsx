import { Navigate, createBrowserRouter, RouterProvider } from 'react-router-dom';
import { SignIn } from './Pages/SignIn'
import { Register } from './Pages/Register';
import { HomePage } from './Pages/HomePage';
import { RootState } from "./state";
import { useSelector } from "react-redux";
import { ProfilePage } from './Pages/ProfilePage';
import { DetailedPostCard } from './Pages/HomePage/DetailedPostCard';

const App = () => {
    const isAuth = Boolean(useSelector<RootState>((state) => state.token));

    const router = createBrowserRouter([
        {
            path: '/',
            element: isAuth ? <HomePage /> : <SignIn />,
        },
        {
            path:'/login',
            element: isAuth ? <Navigate to="/" /> : <SignIn />
        },
        {
            path:'/register',
            element: isAuth ? <Navigate to="/" /> : <Register />
        },
        {
            path: '/profile/:userName',
            element: <ProfilePage />
        },
        {
            path: '/profile/:userName/post/:postId',
            element: <DetailedPostCard />
        }
    ])

    return (
        <>
            <RouterProvider router={router} />
        </>
    );
}

export default App;