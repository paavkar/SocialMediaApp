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
    quotedPost: z.custom<Post>()
})

type PostProps = {
    post?: Post;
    setShowQuoteModal: React.Dispatch<React.SetStateAction<boolean>>;
}

export const QuoteModal = ({ post, setShowQuoteModal }: PostProps) => {
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
                quotedPost: post
            }
        })

    async function onSubmit(values: z.infer<typeof Schema>) {
        let words = values.text.split(" ")
        let urls: string[] = []
        
        for (let word in words) {
            try {
                let potentialUrl = new URL(word)
                urls.push(potentialUrl.href)
            } catch (error) {
                
            }
        }

        if (urls.length > 0)
        {
            if (urls.length === 1) {
                values.embed.externalLink = {
                    externalLinkDescription: "",
                    externalLinkTitle: "",
                    externalLinkUri: urls[0],
                    externalLinkThumbnail: ""
                }
            }
            else {
                values.embed.externalLink = {
                    externalLinkDescription: "",
                    externalLinkTitle: "",
                    externalLinkUri: urls[urls.length-1],
                    externalLinkThumbnail: ""
                }
            }
        }
        
        values.quotedPost.replies = []
        if (values.quotedPost.parentPost) {
            values.quotedPost.parentPost.replies = []
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
            setShowQuoteModal(false)
        }
    }

    function getTimeSinceString(): string {
        const postDate = new Date(post!.createdAt)
        const currentDate = new Date()
        const timeDifference = currentDate.getTime() - postDate.getTime()
        const seconds = timeDifference/1000
        let timeString = ""

        let interval = seconds / 31536000; 
        if (interval > 1) {
            timeString = postDate.toLocaleDateString()
            return timeString
        } 

        interval = seconds / 2592000; 
        if (interval > 1) { 
            Math.floor(interval) > 1
            ? timeString = `${Math.floor(interval)} months ago`
            : timeString = `${Math.floor(interval)} month ago`
            return timeString
        } 

        interval = seconds / 604800; 
        if (interval > 1) {
            Math.floor(interval) > 1
            ? timeString = `${Math.floor(interval)} weeks ago`
            : timeString = `${Math.floor(interval)} week ago`
            return timeString
        } 

        interval = seconds / 86400; 
        if (interval > 1) { 
            Math.floor(interval) > 1
            ? timeString = `${Math.floor(interval)} days ago`
            : timeString = `${Math.floor(interval)} day ago`
            return timeString
        } 

        interval = seconds / 3600; 
        if (interval > 1) {
            Math.floor(interval) > 1
            ? timeString = `${Math.floor(interval)} hours ago`
            : timeString = `${Math.floor(interval)} hour ago`
            return timeString
        } 

        interval = seconds / 60; 
        if (interval > 1) { 
            Math.floor(interval) > 1
            ? timeString = `${Math.floor(interval)} minutes ago`
            : timeString = `${Math.floor(interval)} minute ago`
            return timeString
        } 

        return `${Math.floor(seconds)} seconds ago`
    }

return (
    <div style={{ display: 'block', position: 'fixed', zIndex: 1, left: 0, top: 0, 
                width: '100%', height: '100%', overflow: 'auto', 
                backgroundColor: `rgba(0,0,0,0.4)` }}>
        <div style={{ backgroundColor: '#242424', margin: '15em auto', padding: '20px', border: '1px solid #888',
            width: '30%', height: 'auto', borderRadius: '0.5em' }}>
            <form onSubmit={handleSubmit(onSubmit)}>
                <div style={{ display: 'flex', flexDirection: 'column', gap: '1em', marginTop: '1em' }}>
                    <div style={{ display: 'flex', flexDirection: 'row' }}>
                        <img src="" width={40} height={40} style={{ borderRadius: '50%', margin: '0.5em' }} />
                        <textarea 
                            {...register("text")} 
                            style={{ width: '100%', backgroundColor: '#242424', border: 'none', resize: 'none',
                            height: '6em', fontSize: '17px', outline: 'none', marginTop: '0.5em' }} 
                            placeholder="Write your quote" maxLength={240} />
                    </div>

                    <div style={{ display: 'flex', flexDirection: 'row', border: '1px solid cyan',
                        borderRadius: '0.5em'}}>
                            <div style={{ display: 'flex', marginTop: '0.5em' }}>
                                <div>
                                    <img src="" width={40} height={40} style={{ borderRadius: '50%', margin: '0.5em' }} /> 
                                </div>
                                <div style={{ display: 'flex', flexDirection: 'column', paddingBottom: '1em' }}>
                                    <div style={{ display: 'flex', flexDirection: 'row', gap: '4px' }}>
                                        <span>{post?.author.displayName}</span>
                                        <span> {" @"}{post?.author.userName}</span>
                                        <span> {" "}{getTimeSinceString()}</span>
                                    </div>
                                    <span>{post?.text}</span>
                                </div>
                            </div>
                    </div>
                    
                    <div>
                        <button disabled={isSubmitting} type="submit"
                            style={{ margin: '1em 0em 0.5em 0.5em', backgroundColor: 'green',
                            height: '2em', width: '4em' }}>
                            Quote
                        </button>

                        <button type="button"
                            style={{ margin: '1em 0em 0.5em 0.5em', backgroundColor: 'red',
                            height: '2em', width: '4em' }} onClick={() => setShowQuoteModal(false)}>
                            Cancel
                        </button>
                    </div>
                    
                </div>
            </form>
        </div>
    </div>
    
        
    )
}