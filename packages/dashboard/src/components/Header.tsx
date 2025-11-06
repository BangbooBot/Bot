import { Link } from '@tanstack/react-router'

import { useState } from 'react'
import { Home, Menu, X } from 'lucide-react'
import "@css/navbar.css"

export default function Header() {
  const [isOpen, setIsOpen] = useState(false)

  return (
    <>
      <header className="fixed w-full p-4 flex justify-between items-center bg-black/10 text-white backdrop-blur-md  shadow-lg shadow-white/10 z-30">
        <div className='flex justify-start items-center gap-2'>
          <Link to="/" className="mx-8 flex gap-4 items-center">
            <img
              src="/images/bangboo/Butler.png"
              alt="Bangboo"
              className="h-12 drop-shadow-sm drop-shadow-lime-600"
            />
            <h1 className="text-2xl font-bold">BANGBOO</h1>
          </Link>

          <Link to='/' className='nav-links'>Home</Link>
          <Link to='/' className='nav-links'>Commands</Link>
          <Link to='/' className='nav-links'>About</Link>
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
