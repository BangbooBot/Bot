import { Link } from '@tanstack/react-router'

import { useState } from 'react'
import { Home, Menu, X } from 'lucide-react'
import "@css/navbar.css"

export default function Header() {
  const [isOpen, setIsOpen] = useState(false)

  return (
    <>
      <header className="w-full min-w-0 sticky top-0 inset-x-0 z-30 p-4 xl:px-[calc((100vw-1280px)/2)] flex justify-between items-center bg-black/10 text-white backdrop-blur-md  shadow-lg shadow-white/10">
        <div className='flex justify-start items-center gap-2'>
          <Link to="/" className="mx-8 flex gap-4 items-center">
            <img
              src="/images/bangboo/Penguinboo.png"
              alt="Bangboo"
              className="h-14 drop-shadow-sm drop-shadow-lime-600"
            />
          </Link>
        </div>

        <div className='flex justify-center items-center gap-2'>
          <Link to='/' className='nav-links'>HOME</Link>
          <Link to='/' className='nav-links'>COMMANDS</Link>
          <Link to='/' className='nav-links'>STATUS</Link>
        </div>

        <div className='flex justify-end items-center gap-2'>
          <Link to='/' className='nav-login flex items-center gap-2.5'>
            <img
              src="/icons/power.svg"
              alt="Power icon"
              className="h-5"
            />
            <h6 className='text-lime-300'>Login with Discord</h6>
          </Link>
        </div>
      </header>
    </>
  )
}
