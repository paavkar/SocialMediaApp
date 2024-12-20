import { NavLink } from "react-router"
export default function SignIn() {

    return (
        <div style={{ display:' flex', alignItems:'center', flexDirection: 'column', width: '90vw', marginLeft: '5em' }}>
            <div style={{ marginBottom: '1em'}}>
                <h1>Sign in</h1>
            </div>
            <div style={{ display:'flex', flexDirection: 'column' }}>

                <label style={{ marginBottom: '0.2em', fontSize: '20px' }} 
                       htmlFor="emailOrUserName">
                        Email or Username
                </label>

                <input style={{ width: '20em', height: '2em' }} 
                       type="text" 
                       id="emailOrUserName"
                       placeholder="Email or Username">
                </input>

                <label style={{ marginBottom: '0.2em', marginTop: '1em', fontSize: '20px' }} 
                       htmlFor="password">
                        Password
                </label>

                <input style={{ width: '20em', height: '2em' }} 
                       type="password" 
                       id="password"
                       placeholder="Password">
                </input>
                
                <button style={{ marginTop: '1em', backgroundColor: 'green' }}>
                    Sign in
                </button>
            </div>

            <div style={{ display:'flex', flexDirection: 'column', marginTop: '1em' }}>
               <NavLink to="/register">Don't have an account? Click here to register.</NavLink> 
            </div>
            
        </div>
    )
}