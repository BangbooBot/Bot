import Image from "next/image";
import Link from 'next/link';
//import { useRouter } from 'next/router';
import { Auth } from "@components";
import "@css/navbar.css";

export function MainNavBar() {
    return(
        <nav className="relative w-full h-full flex justify-between items-center px-8">
            <Link href={"/"} className="w-52 h-fit flex justify-center">
                <Image src="/images/bangboos/Butler.webp" alt="Butler" width={64} height={64}/>
            </Link>

            <div className="flex justify-between gap-12">
                <Link href={"/"} className="nav-links">Home</Link>
                <Link href={"/commands"} className="nav-links">Commands</Link>
                <Link href={"/about"} className="nav-links">About</Link>
            </div>

            <div className="w-52 flex justify-center">
                <Auth/>
            </div>
            
        </nav>
    )
}