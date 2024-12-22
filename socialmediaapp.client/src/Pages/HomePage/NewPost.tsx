import { useSelector } from "react-redux";
import { User } from '../../types';
import { RootState } from "../../state";
import { z } from "zod";
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { Author } from "../../types";
import { useNavigate } from "react-router";

const Schema = z.object({
    text: z.string().min(1),
    author: z.custom<Author>(),
    langs: z.custom<string[]>()
})

export function NewPost() {
    const user = useSelector<RootState, User | null>((state) => state.user);
    const token = useSelector<RootState, string | null>((state) => state.token);
    
    const navigate = useNavigate();
    const { register, handleSubmit, formState: { isSubmitting }, reset} = useForm<z.infer<typeof Schema>>({
            resolver: zodResolver(Schema),
            defaultValues: {
                text: "",
                author: {
                    id: user?.id,
                    displayName: user?.displayName,
                    userName: user?.userName,
                },
                langs: ["en"],
            }
        })

    async function onSubmit(values: z.infer<typeof Schema>) {
        var response = await fetch("api/Post/post", {
            method: "POST",
            body: JSON.stringify(values),
            headers: {
                "Content-Type": "application/json",
                "Authorization": `bearer ${token}`
            }
        })

        if (response.ok) {
            reset()
            navigate("/")
        }
    }

return (
    <div>
        <form onSubmit={handleSubmit(onSubmit)}>
            <textarea 
                {...register("text")} 
                placeholder="What's happening?"
                rows={8}
                style={{ fontSize: '17px', resize: 'none', width: '100%', padding: '0px' }} />
            <button disabled={isSubmitting} type="submit"
                    style={{ margin: '1em 0em 0.2em 0.5em', backgroundColor: 'green',
                        height: '2em', width: '4em'
                     }}>
                Post
            </button>
        </form>
        <hr style={{ borderColor: 'cyan' }} />
    </div>
        
    )
}