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
        let words = values.text.split(/\s+/)
        let urls: string[] = []
        
        for (let word of words) {
            try {
                if (!word.includes("http")) {
                    continue
                }
                let potentialUrl = new URL(word)
                let url = potentialUrl.href
                urls.push(url)
            } catch (error) {
                
            }
        }

        if (urls.length > 0)
        {
            if (urls.length === 1) {
                values.embed.embedType = EmbedType.ExternalLink
                values.embed.externalLink = {
                    externalLinkDescription: "",
                    externalLinkTitle: "",
                    externalLinkUri: urls[0],
                    externalLinkThumbnail: ""
                }
            }
            else {
                values.embed.embedType = EmbedType.ExternalLink
                values.embed.externalLink = {
                    externalLinkDescription: "",
                    externalLinkTitle: "",
                    externalLinkUri: urls[urls.length-1],
                    externalLinkThumbnail: ""
                }
            }
        }
        console.log(values.embed.externalLink?.externalLinkUri)
        
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