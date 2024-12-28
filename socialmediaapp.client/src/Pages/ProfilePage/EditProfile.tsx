import { ChangeEvent, useState } from "react"
import { useSelector, useDispatch } from "react-redux";
import { RootState, setUser } from "../../state";
import { User } from "../../types";
import Layout from "../layout";
import { useNavigate } from "react-router";

type PostProps = {
    setShowEdit: React.Dispatch<React.SetStateAction<boolean>>;
}

export const EditProfile = ({ setShowEdit }: PostProps) => {
    const token = useSelector<RootState, string | null>((state) => state.token);
    const loggedInUser = useSelector<RootState, User | null>((state) => state.user);
    const [selectedFile, setSelectedFile] = useState<File | null>(null);

    const fileSelectedHandler = (event: ChangeEvent<HTMLInputElement>) => { 
        if (event.target.files && event.target.files.length > 0) { 
            setSelectedFile(event.target.files[0]); 
        };
    }

    const uploadHandler = async () => { 
        if (!selectedFile) { return; }
        const formData = new FormData(); 
        formData.append('image', selectedFile); 
        try { 
            const response = await fetch(`/api/User/upload-profile-picture/${loggedInUser?.id}`, {
                method: 'POST',
                body: formData,
                headers: {
                    'Authorization': `bearer ${token}`
                }
            });
            console.log(response.status)

            if (response.ok) {
                const result = await response.json()
                setUser({ ...loggedInUser, profilePicture: result.url })
            }
        } 
            catch (error) 
            { 
                console.error(error); 
            }
    }

    return (
        <div style={{ display: 'block', position: 'fixed', zIndex: 1, left: 0, top: 0, 
            width: '100%', height: '100%', overflow: 'auto', 
            backgroundColor: `rgba(0,0,0,0.4)` }}>
            <div style={{ backgroundColor: '#242424', margin: '15em auto', padding: '20px', border: '1px solid #888',
                width: '30%', height: 'auto', borderRadius: '0.5em' }}>
                <div>
                    <img src={loggedInUser?.profilePicture} width={70} height={70} style={{ borderRadius: '50%' }}></img>
                    <input type="file" onChange={fileSelectedHandler} />
                    <div>
                        <button type="button" onClick={uploadHandler} style={{ margin: '1em 0em 0.5em 0.5em', backgroundColor: 'green',
                            height: '2em', width: '15em' }}>
                            Confirm profile picture change
                        </button>

                        <button type="button"
                            style={{ margin: '1em 0em 0.5em 0.5em', backgroundColor: 'red',
                            height: '2em', width: '4em' }} onClick={() => setShowEdit(false)}>
                            Close
                        </button>
                    </div>
                    
                </div>
            </div>
        </div>
    )
}