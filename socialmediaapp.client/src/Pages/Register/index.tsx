import { useForm } from "react-hook-form"
import { NavLink } from "react-router"
import { z } from "zod"
import { zodResolver } from "@hookform/resolvers/zod"
import { useState } from "react"
import { User } from "../../types"

const Schema = z.object({
    email: z.string().min(1, { message: "Email cannot be empty" }).email({ message: "Email must be of valid form" }),
    displayName: z.string().min(1, { message: "Display name cannot be empty" }),
    userName: z.string().min(1, {  message: "Username cannot be empty" }),
    password: z.string().min(8, { message: "Password must be at least 8 characters long" }),
    confirmPassword: z.string()
}).refine(data => data.password === data.confirmPassword, {
    message: "Passwords must match",
    path: ["confirmPassword"]
})

export default function Register() {
    const { register, handleSubmit, formState: { errors, isSubmitting }, reset} = useForm<z.infer<typeof Schema>>({
        resolver: zodResolver(Schema),
        defaultValues: {
            email: "",
            displayName: "",
            userName: "",
            password: "",
            confirmPassword: ""
        }
    })
    const [httpError, setHttpError] = useState("");
    const [httpSuccess, setHttpSuccess] = useState("");

    async function onSubmit(values: z.infer<typeof Schema>) {
        var response = await fetch("api/Auth/register", {
            method: "POST",
            body: JSON.stringify(values),
            headers: {
                "Content-Type": "application/json"
            }
        })

        if (response.status === 400) {
            var error = await response.json()
            setHttpError(error.message)

            setTimeout(() => setHttpError(""), 5000)
        }
        else {
            var user: User = await response.json()
            setHttpSuccess(`User created with email ${user.email} and username ${user.userName}`)

            reset()
        }
    }

    return (
        <div style={{ display:' flex', alignItems:'center', flexDirection: 'column', width: '90vw', marginLeft: '5em' }}>
            <div style={{ marginBottom: '1em'}}>
                <h1>Register</h1>
            </div>
            <form onSubmit={handleSubmit(onSubmit)}>
            
            {httpError && 
            <p style={{ color: 'red', width: '17em', height: '2em', borderRadius: '0.2em' }}>{httpError}</p>}

            {httpSuccess && 
            <p style={{ color: 'green', width: '17em', height: '2em', borderRadius: '0.2em' }}>{httpSuccess}</p>}

            <div style={{ display:'flex', flexDirection: 'column' }}>
                <label style={{ marginBottom: '0.2em', fontSize: '20px' }} 
                       htmlFor="email">
                        Email
                </label>

                {errors.email && (
                    <p style={{ color: 'red', width: '17em', height: '2em', borderRadius: '0.2em' }}>
                        {`${errors.email.message}`}
                    </p>
                )}

                <input style={{ width: '20em', height: '2em' }}
                       {...register("email")}
                       type="email" 
                       id="email"
                       placeholder="Email">
                </input>

                <label style={{ marginBottom: '0.2em', marginTop: '1em', fontSize: '20px' }} 
                       htmlFor="userName">
                        Username
                </label>

                {errors.userName && (
                    <p style={{ color: 'red', width: '17em', height: '2em', borderRadius: '0.2em' }}>
                        {`${errors.userName.message}`}
                    </p>
                )}

                <input style={{ width: '20em', height: '2em' }} 
                       {...register("userName")}
                       type="text" 
                       id="userName"
                       placeholder="Username">
                </input>

                <label style={{ marginBottom: '0.2em', marginTop: '1em', fontSize: '20px' }} 
                       htmlFor="displayName">
                        Display name
                </label>

                {errors.displayName && (
                    <p style={{ color: 'red', width: '17em', height: '2em', borderRadius: '0.2em' }}>
                        {`${errors.displayName.message}`}
                    </p>
                )}

                <input style={{ width: '20em', height: '2em' }} 
                       {...register("displayName")}
                       type="text" 
                       id="displayName"
                       placeholder="Display name">
                </input>

                <label style={{ marginBottom: '0.2em', marginTop: '1em', fontSize: '20px' }} 
                       htmlFor="password">
                        Password
                </label>

                {errors.password && (
                    <p style={{ color: 'red', width: '17em', height: '2em', borderRadius: '0.2em' }}>
                        {`${errors.password.message}`}
                    </p>
                )}

                <input style={{ width: '20em', height: '2em' }} 
                       {...register("password")}
                       type="password" 
                       id="password"
                       placeholder="Password">
                </input>

                <label style={{ marginBottom: '0.2em', marginTop: '1em', fontSize: '20px' }} 
                       htmlFor="confirmPassword">
                        Confirm password
                </label>

                {errors.confirmPassword && (
                    <p style={{ color: 'red', width: '17em', height: '2em', borderRadius: '0.2em' }}>
                        {`${errors.confirmPassword.message}`}
                    </p>
                )}

                <input style={{ width: '20em', height: '2em' }} 
                       {...register("confirmPassword")}
                       type="password" 
                       id="confirmPassword"
                       placeholder="Confirm password">
                </input>

                <button type="submit" 
                    disabled={isSubmitting} 
                    style={{ marginTop: '1em', backgroundColor: 'green' }}>
                    Register
                </button>
            </div>

            <div>
                <div style={{ display:'flex', flexDirection: 'column', marginTop: '1em' }}>
                    <span>Already have an account?</span>
                   <NavLink to="/login">Sign in here.</NavLink> 
                </div>
            </div>
            </form>
        </div>
    )
}