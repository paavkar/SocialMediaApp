import { useSelector } from "react-redux";
import { Embed, EmbedType, User } from '../../types';
import { RootState } from "../../state";
import { z } from "zod";
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { Author, Post } from "../../types";

const Schema = z.object({
    text: z.string().min(1),
    author: z.custom<Author>(),
    langs: z.custom<string[]>(),
    embed: z.custom<Embed>(),
    parentPost: z.custom<Post>()
})

type PostProps = {
    post?: Post;
    setShowModal: React.Dispatch<React.SetStateAction<boolean>>;
    addToPostReplies: (reply: Post) => void;
}

export const ReplyModal = ({ post, setShowModal, addToPostReplies }: PostProps) => {
    const user = useSelector<RootState, User | null>((state) => state.user);
    const token = useSelector<RootState, string | null>((state) => state.token);
    
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
                },
                parentPost: post
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

        values.parentPost.replies = []

        var response = await fetch("/api/Post/post", {
            method: "POST",
            body: JSON.stringify(values),
            headers: {
                "Content-Type": "application/json",
                "Authorization": `bearer ${token}`
            }
        })

        if (response.ok) {
            var replyAndPosts = await response.json()
            addToPostReplies(replyAndPosts.createdPost)
            reset()
            setShowModal(false)
        }
    }

return (
    <div style={{ display: 'block', position: 'fixed', zIndex: 1, left: 0, top: 0, 
                width: '100%', height: '100%', overflow: 'auto', 
                backgroundColor: `rgba(0,0,0,0.4)` }}>
        <div style={{ backgroundColor: '#242424', margin: '15em auto', padding: '20px', border: '1px solid #888',
            width: '30%', height: 'auto', borderRadius: '0.5em' }}>
            <div style={{ display: 'flex', flexDirection: 'row', borderBottom: '1px solid cyan' }}>
                <div>
                    <img src="" width={40} height={40} style={{ borderRadius: '50%', margin: '0.5em' }} /> 
                </div>
                <div style={{ display: 'flex', flexDirection: 'column', paddingBottom: '1em' }}>
                    <span>{post?.author.displayName}</span>
                    <span>{post?.text}</span>
                </div>
            </div>
            <form onSubmit={handleSubmit(onSubmit)}>
                <div style={{ display: 'flex', flexDirection: 'column', gap: '1em', marginTop: '1em' }}>
                    <div style={{ display: 'flex', flexDirection: 'row' }}>
                        <img src="" width={40} height={40} style={{ borderRadius: '50%', margin: '0.5em' }} />
                        <textarea 
                            {...register("text")} 
                            style={{ width: '100%', backgroundColor: '#242424', border: 'none', resize: 'none',
                            height: '6em', fontSize: '17px', outline: 'none', marginTop: '0.5em' }} 
                            placeholder="Write your reply" maxLength={240} />
                    </div>
                    
                    <div>
                        <button disabled={isSubmitting} type="submit"
                            style={{ margin: '1em 0em 0.5em 0.5em', backgroundColor: 'green',
                            height: '2em', width: '4em' }}>
                            Reply
                        </button>

                        <button type="button"
                            style={{ margin: '1em 0em 0.5em 0.5em', backgroundColor: 'red',
                            height: '2em', width: '4em' }} onClick={() => setShowModal(false)}>
                            Cancel
                        </button>
                    </div>
                    
                </div>
            </form>
        </div>
    </div>
    
        
    )
}