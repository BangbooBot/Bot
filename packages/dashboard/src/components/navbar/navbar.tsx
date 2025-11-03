import Image from "next/image";
import Link from 'next/link';
//import { useRouter } from 'next/router';
import { Auth } from "@components";

export function MainNavBar() {
    return(
        <nav className="relative w-full h-full flex justify-between items-center px-8">
            <Link href={"/"} className="w-20 h-fit flex justify-start">
                <Image src="/images/bangboos/Butler.webp" alt="Butler" width={64} height={64}/>
            </Link>

            <div className="flex gap-12">
                <Link href={"/"}>Home</Link>
                <Link href={"/commands"}>Commands</Link>
                <Link href={"/about"}>About</Link>
            </div>

            <div className="w-20 flex justify-end">
                <Auth/>
            </div>
            
        </nav>
    )
}