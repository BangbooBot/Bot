import Image from "next/image";
import Link from 'next/link';
//import { useRouter } from 'next/router';

export function Auth() {
    return(
        <Link href={"/login"} className="">Login</Link>
    )
}