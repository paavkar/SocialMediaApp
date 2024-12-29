import { useSelector } from "react-redux";
import { Embed, EmbedType, Media, User } from '../../types';
import { RootState } from "../../state";
import { z } from "zod";
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { Author, Post } from "../../types";
import { ChangeEvent, useState } from "react";

const Schema = z.object({
    text: z.string().min(1),
    author: z.custom<Author>(),
    langs: z.custom<string[]>(),
    embed: z.custom<Embed>()
})

interface NewPostProps {
    addToPosts: (post: Post) => void
}

interface SelectedFile { 
    file: File;
    width: number;
    height: number;
    altText: string;
}

export const NewPost = ({ addToPosts }: NewPostProps) => {
    const user = useSelector<RootState, User | null>((state) => state.user);
    const token = useSelector<RootState, string | null>((state) => state.token);
    const [selectedFiles, setSelectedFiles] = useState<SelectedFile[]>([]);
    const [errorMessage, setErrorMessage] = useState<string | null>()

    const { register, handleSubmit, formState: { isSubmitting }, reset} = useForm<z.infer<typeof Schema>>({
            resolver: zodResolver(Schema),
            defaultValues: {
                text: "",
                author: {
                    id: user?.id,
                    profilePicture: user?.profilePicture,
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

        if (selectedFiles.length > 0) {
            values.embed.embedType = EmbedType.Images
            let images: Media[] = []

            for (let file of selectedFiles) {
                images.push({ altText: file.altText, aspectRatio: { width: file.width, height: file.height }})
            }

            values.embed.images = images
        }

        if (urls.length > 0 && selectedFiles.length == 0)
        {
            values.embed.embedType = EmbedType.ExternalLink
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
        
        var response = await fetch("/api/Post/post", {
            method: "POST",
            body: JSON.stringify(values),
            headers: {
                "Content-Type": "application/json",
                "Authorization": `bearer ${token}`
            }
        })

        if (response.ok) {
            const postResponse = await response.json()
            const post = postResponse.createdPost

            if (selectedFiles.length === 0) {
                return;
            }
            const formData = new FormData(); 

            selectedFiles.forEach((selectedFile) => { 
                formData.append('images', selectedFile.file);
            });
    
            try {
                const imageResponse = await fetch(`/api/Post/upload-post-images/${post.id}`, {
                    method: 'POST',
                    body: formData,
                    headers: {
                        "Authorization": `bearer ${token}`
                    }
                });

                const data = await imageResponse.json();
                reset()
                addToPosts(data.createdPost)
            } catch (error) {
                console.error('There was an error!', error);
            }
        }
    }

    const fileSelectedHandler = (event: ChangeEvent<HTMLInputElement>) => { 
        const files = event.target.files; 
        if (files && files.length > 4) { 
            setErrorMessage('You can upload a maximum of 4 files.'); 
            return; 
        } 
        const fileArray = files 
        ? Array.from(files) 
        : [];

        const selectedFileDetails: SelectedFile[] = [];

        fileArray.forEach((file) => { 
            const img = new Image(); 
            img.src = URL.createObjectURL(file); 
            img.onload = () => { 
                selectedFileDetails.push({ file: file, width: img.width, height: img.height, altText: file.name });
                if (selectedFileDetails.length === fileArray.length) { 
                    setSelectedFiles(selectedFileDetails); 
                } 
            }; 
        }); 
        setErrorMessage(null); 
    };

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
                <div>
                    <input type="file" multiple onChange={fileSelectedHandler} />
                    {errorMessage && <p style={{ color: 'red' }}>
                        {errorMessage}
                        </p>}
                </div>
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