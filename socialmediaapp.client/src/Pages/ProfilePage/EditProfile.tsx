import { ChangeEvent, useState, useRef } from "react"
import { useSelector, useDispatch } from "react-redux";
import { RootState, setUser } from "../../state";
import { User } from "../../types";
import { z } from "zod";
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod";
import 'cropperjs/dist/cropper.css'
import Cropper, { ReactCropperElement } from 'react-cropper'

type PostProps = {
    setShowEdit: React.Dispatch<React.SetStateAction<boolean>>;
    displayedUser: User | undefined;
    setDisplayedUser: React.Dispatch<React.SetStateAction<User | undefined>>
}

const Schema = z.object({
    displayName: z.string().min(1, { message: "Display name cannot be empty" }),
    description: z.string(),
})

export const EditProfile = ({ setShowEdit, displayedUser, setDisplayedUser }: PostProps) => {
    const token = useSelector<RootState, string | null>((state) => state.token);
    const loggedInUser = useSelector<RootState, User | null>((state) => state.user);
    const [selectedFile, setSelectedFile] = useState<File | null>(null);
    const [croppedDataUrl, setCroppedDataUrl] = useState<string | null>(null);
    const [imageDimensions, setImageDimensions] = useState<{ width: number; height: number } | null>(null);
    const [errorMessage, setErrorMessage] = useState<string | null>(null);
    const cropperRef = useRef<ReactCropperElement>(null)
    const dispatch = useDispatch();

    const { register, handleSubmit, formState: { errors, isSubmitting }} = useForm<z.infer<typeof Schema>>({
                resolver: zodResolver(Schema),
                defaultValues: {
                    displayName: loggedInUser?.displayName,
                    description: loggedInUser?.description,
                }
        })

    const fileSelectedHandler = (event: ChangeEvent<HTMLInputElement>) => {
        const file = event.target.files?.[0]
        if (!file) return

        const img = new Image()
        img.src = URL.createObjectURL(file)
        img.onload = () => {
            setSelectedFile(file)
            setImageDimensions({ width: img.width, height: img.height })
            setErrorMessage(null)
        }
    }

    const getCroppedImage = () => {
        const cropper = cropperRef.current?.cropper
        if (cropper) {
            const croppedDataUrl = cropper.getCroppedCanvas().toDataURL()
            setCroppedDataUrl(croppedDataUrl)
        }
    }

    const uploadHandler = async () => { 
        if (!selectedFile) { 
            return; 
        }
        
        const formData = new FormData();
        if (croppedDataUrl) {
            const byteString = atob(croppedDataUrl.split(',')[1])
            const mimeString = croppedDataUrl.split(',')[0].split(':')[1].split(';')[0]
            const ab = new ArrayBuffer(byteString.length)
            const ia = new Uint8Array(ab)

            for (let i = 0; i < byteString.length; i++) {
                ia[i] = byteString.charCodeAt(i)
            }
            const blob = new Blob([ab], { type: mimeString })
            formData.append('image', blob, selectedFile.name);
        } else {
            formData.append('image', selectedFile)
        }
        try { 
            const response = await fetch(`/api/User/upload-profile-picture/${loggedInUser?.id}`, {
                method: 'POST',
                body: formData,
                headers: {
                    'Authorization': `bearer ${token}`
                }
            });

            if (response.ok) {
                const result = await response.json()
                dispatch(setUser({ user: { ...loggedInUser, profilePicture: result.url }}))
            }
        } 
        catch (error) 
        {

        }
    }

    async function onSubmit(values: z.infer<typeof Schema>) {
        const author = {
            id: loggedInUser?.id,
            profilePicture: loggedInUser?.profilePicture,
            displayName: values.displayName,
            userName: loggedInUser?.userName,
            description: values.description
        }
        var response = await fetch(`/api/User/update-user/${loggedInUser?.id}`, {
            method: "PATCH",
            body: JSON.stringify(author),
            headers: {
                "Content-Type": "application/json",
                "Authorization": `bearer ${token}`
            }
        })

        if (response.ok) {
            const user = await response.json()
            dispatch(setUser({user: user}))
            if (user.userName === displayedUser?.userName) {
                setDisplayedUser(user)
            }
            await uploadHandler()
        }
    }

    return (
        <div style={{ display: 'block', position: 'fixed', zIndex: 1, left: 0, top: 0, 
            width: '100%', height: '100%', overflow: 'auto', 
            backgroundColor: `rgba(0,0,0,0.4)` }}>
            <div style={{ backgroundColor: '#242424', margin: '15em auto', padding: '20px', border: '1px solid #888',
                width: '30%', height: 'auto', borderRadius: '0.5em' }}>
                <div>
                    <img src={loggedInUser?.profilePicture} width={70} height={70} style={{ borderRadius: '50%' }} />
                    <input type="file" onChange={fileSelectedHandler} />
                    <div>
                        {selectedFile && (
                            <Cropper src={URL.createObjectURL(selectedFile)}
                            style={{ maxWidth: 400 }}
                            initialAspectRatio={1}
                            guides={false}
                            ref={cropperRef} />
                        )}
                        <button onClick={getCroppedImage}>Crop</button>
                    </div>
                    <div>
                        <form onSubmit={handleSubmit(onSubmit)}>
                            <div style={{ display: 'flex', flexDirection: 'column' }}>
                                <label style={{ marginBottom: '0.2em', fontSize: '16px' }} 
                                    htmlFor="displayName">
                                    Display name
                                </label>

                                {errors.displayName && (
                                    <p style={{ color: 'red', width: '17em', height: '2em', borderRadius: '0.2em' }}>
                                        {`${errors.displayName.message}`}
                                    </p>
                                )}

                                <input style={{ width: '20em', height: '2em', borderRadius: '0.5em' }}
                                    {...register("displayName")}
                                    type="text" 
                                    id="displayName"
                                    placeholder="">
                                </input>
                            </div>

                            <div style={{ display: 'flex', flexDirection: 'column' }}>
                                <label style={{ marginBottom: '0.2em', fontSize: '16px' }} 
                                    htmlFor="description">
                                    Description
                                </label>

                                <textarea style={{ borderRadius: '0.5em', height: '12em', width: '30em', resize: 'none' }}
                                    {...register("description")}
                                    id="description"
                                    maxLength={440}
                                    placeholder="">
                                </textarea>
                            </div>

                            <div style={{display: 'flex', flexDirection: 'row'}}>
                                <button type="submit" disabled={isSubmitting} style={{ margin: '1em 0em 0.5em 0.5em', backgroundColor: 'green',
                                    height: '2em', width: '10em' }}>
                                    Confirm changes
                                </button>

                                <button type="button"
                                    style={{ margin: '1em 0em 0.5em 0.5em', backgroundColor: 'red',
                                    height: '2em', width: '4em' }} onClick={() => setShowEdit(false)}>
                                    Close
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            </div>
        </div>
    )
}