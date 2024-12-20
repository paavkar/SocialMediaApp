import { BrowserRouter, Routes, Route } from 'react-router';
import SignIn from './Pages/SignIn'
import Register from './Pages/Register';

function App() {

    return (
        <div>
            <BrowserRouter>
            <Routes>
                <Route path='/' element={ <SignIn /> } />
                <Route path='/login' element={ <SignIn /> } />
                <Route path='/register' element={ <Register /> } />
            </Routes>
            </BrowserRouter>
        </div>
    );
}

export default App;