import { Link } from '@tanstack/react-router'

import { useState } from 'react'
import '@css/riducard.css'

export default function Card() {
  return (
    <>
      <div className="card bg-card">
        <section className="card-section-owner bg-card-section">
          <h1 className="text-card">Ridu Newslater</h1>
        </section>
        <section className="card-section-title bg-card-section">
          <h1 className="font-[Anton] text-card">BANGBOO BOT</h1>
        </section>

        <section className="card-section-intro bg-card-section">
          <div className='border-card'>
            <img src='/images/bangboo/18.png'/>
            <div className='bg-card'>
              <p className='text-card-section'>ENN ENNEN</p>
            </div>
          </div>

          <div>
            <h1 className='font-[Anton] text-card'>SMALL BODY, BIG HELPPER!</h1>
            <p className='font-[Anton] text-card'>Bot voltado para comunidade designado a entreter e proteger os proxys do seu servidor.</p>
          </div>
        </section>

        <section className="card-section-info bg-card-section">
          <div>
            <div>
              <img src="/icons/card-shield.svg" alt="Twitch"/>
              <h5>Moderação</h5>
            </div>
            <div>
              <img src="/icons/card-role.svg" alt="Twitch"/>
              <h5>Cargos</h5>
            </div>
            <div>
              <img src="/icons/card-language.svg" alt="Twitch"/>
              <h5>Idioma</h5>
            </div>
            <div>
              <img src="/icons/card-luck.svg" alt="Twitch"/>
              <h5>Sorteios</h5>
            </div>
            <div>
              <img src="/icons/card-twitch.svg" alt="Twitch"/>
              <h5>Twitch</h5>
            </div>
          </div>
          
          <div>
            <div>
              <div className='bg-card'><p className='text-card-section'>400</p></div>
              <h5>Servidores</h5>
            </div>
            <div>
              <div className='bg-card'><p className='text-card-section'>20</p></div>
              <h5>Comandos</h5>
            </div>
          </div>
        </section>
      </div>
    </>
  )
}
