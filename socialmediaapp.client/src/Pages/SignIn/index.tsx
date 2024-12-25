import { NavLink } from "react-router-dom";
import {z} from "zod";
import { zodResolver } from "@hookform/resolvers/zod"
import { useForm } from "react-hook-form"
import { useState } from "react"
import { useDispatch } from "react-redux";
import { setLogin } from "../../state";
import { useNavigate } from "react-router";

const Schema = z.object({
    emailOrUserName: z.string().min(1, {  message: "Write your email or username" }),
    password: z.string().min(1, { message: "Write your password" }),
})

export const SignIn = () => {
    const dispatch = useDispatch();
    const navigate = useNavigate();

    const { register, handleSubmit, formState: { errors, isSubmitting }, reset } = useForm<z.infer<typeof Schema>>({
        resolver: zodResolver(Schema),
        defaultValues: {
            emailOrUserName: "",
            password: "",
        }
    })
    const [httpError, setHttpError] = useState("");

    async function onSubmit(values: z.infer<typeof Schema>) {
            var response = await fetch("api/Auth/login", {
                method: "POST",
                body: JSON.stringify(values),
                headers: {
                    "Content-Type": "application/json"
                }
            })
    
            if (!response.ok) {
                var error = await response.json()
                setHttpError(error.message)
    
                setTimeout(() => setHttpError(""), 5000)
            }
            if (response.ok) {
                var userAndToken = await response.json()

                dispatch(setLogin({ user: userAndToken.user, token: userAndToken.token }))
                reset()
                navigate("/")
            }
        }

    return (
        <div style={{ display:' flex', alignItems:'center', flexDirection: 'column', width: '90vw', marginLeft: '5em' }}>
            <div style={{ marginBottom: '1em'}}>
                <h1>Sign in</h1>
            </div>
            <form onSubmit={handleSubmit(onSubmit)}>
                {httpError && 
                <p style={{ color: 'red', width: '17em', height: '2em', borderRadius: '0.2em' }}>{httpError}</p>}

                <div style={{ display:'flex', flexDirection: 'column' }}>

                    <label style={{ marginBottom: '0.2em', fontSize: '20px' }} 
                        htmlFor="emailOrUserName">
                            Email or Username
                    </label>

                    {errors.emailOrUserName && (
                        <p style={{ color: 'red', width: '17em', height: '2em', borderRadius: '0.2em' }}>
                            {`${errors.emailOrUserName.message}`}
                        </p>
                    )}

                    <input style={{ width: '20em', height: '2em' }}
                        {...register("emailOrUserName")}
                        type="text" 
                        id="emailOrUserName"
                        placeholder="Email or Username">
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
                    
                    <button disabled={isSubmitting} type="submit" 
                        style={{ marginTop: '1em', backgroundColor: 'green', width: '17em',
                            height: '2.5em'
                         }}>
                        Sign in
                    </button>
                </div>

                <div style={{ display:'flex', flexDirection: 'column', marginTop: '1em' }}>
                    <span>Don't have an account?</span>
                    <NavLink to="/register"> Click here to register.</NavLink> 
                </div>
            </form>
        </div>
    )
}