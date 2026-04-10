import { Photo } from "./photo";

export interface Member {
    UserName: string;
    avatarUrl: string;
    First_Name: string;
    Last_Name: string;
    Email: string;
    Money: number;
    Created: string;
    LastActive: string;
    Avatar: Photo;
}

