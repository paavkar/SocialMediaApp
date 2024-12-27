import { useSelector } from "react-redux";
import { Embed, EmbedType, User } from '../../types';
import { RootState } from "../../state";
import { z } from "zod";
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { Author, Post } from "../../types";
import { useNavigate } from "react-router-dom";

const Schema = z.object({
    text: z.string().min(1),
    author: z.custom<Author>(),
    langs: z.custom<string[]>(),
    embed: z.custom<Embed>()
})

interface NewPostProps {
    addToPosts: (post: Post) => void
}

export const NewPost = ({ addToPosts }: NewPostProps) => {
    const user = useSelector<RootState, User | null>((state) => state.user);
    const token = useSelector<RootState, string | null>((state) => state.token);
    const navigate = useNavigate();

    const { register, handleSubmit, formState: { isSubmitting }, reset} = useForm<z.infer<typeof Schema>>({
            resolver: zodResolver(Schema),
            defaultValues: {
                text: "",
                author: {
                    id: user?.id,
                    profilePicture: "",
                    displayName: user?.displayName,
                    description: user?.description,
                    userName: user?.userName,
                },
                langs: ["en"],
                embed: {
                    embedType: EmbedType.None,
                }
            }
    })

    async function onSubmit(values: z.infer<typeof Schema>) {
        if (values.text.includes("https://")) {
            values.embed.embedType = EmbedType.ExternalLink

            const indexOfLink = values.text.indexOf("https://")
            const indexOfSpace = values.text.indexOf(" ", indexOfLink)
            let url = values.text.substring(indexOfLink)

            if (indexOfSpace != -1) {
                url = values.text.substring(indexOfLink, indexOfSpace)
            }

            const indexOfComma = url.indexOf(",")
            if (indexOfComma != -1) {
                url = url.substring(0, indexOfComma)
            }
            
            // const indexOfPeriod = url.indexOf(".", url.length)
            // if (indexOfPeriod != -1) {
            //     url = url.substring(0, indexOfPeriod)
            // }

            values.embed.externalLink = {
                externalLinkDescription: "",
                externalLinkTitle: "",
                externalLinkUri: url,
                externalLinkThumbnail: ""
            }
        }
        
        var response = await fetch("/api/Post/post", {
            method: "POST",
            body: JSON.stringify(values),
            headers: {
                "Content-Type": "application/json",
                "Authorization": `bearer ${token}`
            }
        })

        if (response.ok) {
            reset()
            var postResponse = await response.json()
            addToPosts(postResponse.createdPost)
        }
    }

return (
    <div style={{ borderBottom: '1px solid cyan', paddingBottom: '1em' }}>
        <form onSubmit={handleSubmit(onSubmit)}>
            <textarea 
                {...register("text")} 
                placeholder=" What's happening?"
                rows={8}
                style={{ fontSize: '17px', resize: 'none', width: '100%', padding: '0px',
                    backgroundColor: '#242424', border: "none", outline: 'none',
                    borderBottom: '1px solid cyan'
                 }} />
            <button disabled={isSubmitting} type="submit"
                    style={{ margin: '1em 0em 0.2em 0.5em', backgroundColor: 'green',
                        height: '2em', width: '4em'
                     }}>
                Post
            </button>
        </form>
    </div>
        
    )
}