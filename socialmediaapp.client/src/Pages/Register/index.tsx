import { NavLink } from "react-router"

export default function Register() {

    return (
        <div style={{ display:' flex', alignItems:'center', flexDirection: 'column', width: '90vw', marginLeft: '5em' }}>
            <div style={{ marginBottom: '1em'}}>
                <h1>Register</h1>
            </div>
            <div style={{ display:'flex', flexDirection: 'column' }}>

                <label style={{ marginBottom: '0.2em', fontSize: '20px' }} 
                       htmlFor="email">
                        Email
                </label>

                <input style={{ width: '20em', height: '2em' }} 
                       type="text" 
                       id="email"
                       placeholder="Email">
                </input>

                <label style={{ marginBottom: '0.2em', marginTop: '1em', fontSize: '20px' }} 
                       htmlFor="userName">
                        Username
                </label>

                <input style={{ width: '20em', height: '2em' }} 
                       type="text" 
                       id="userName"
                       placeholder="Username">
                </input>

                <label style={{ marginBottom: '0.2em', marginTop: '1em', fontSize: '20px' }} 
                       htmlFor="displayName">
                        Display name
                </label>

                <input style={{ width: '20em', height: '2em' }} 
                       type="text" 
                       id="displayName"
                       placeholder="Display name">
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
                    Register
                </button>
            </div>

            <div>
            <div style={{ display:'flex', flexDirection: 'column', marginTop: '1em' }}>
               <NavLink to="/login">Already have an account? Sign in here.</NavLink> 
            </div>
            </div>
            
        </div>
    )
}