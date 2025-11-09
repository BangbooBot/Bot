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
          <div className="border-card">
            <img src="/images/bangboo/18.png" />
            <div className="bg-card">
              <p className="text-card-section">ENN ENNEN</p>
            </div>
          </div>

          <div>
            <h1 className="font-[Anton] text-card">SMALL BODY, BIG HELPPER!</h1>
            <p className="font-[Anton] text-card">
              Todo servidor precisa de um coelho autonomo para guia-lo ate mesmo
              nos cantos mais escuros. Moderação, Comandos divertidos,
              Notificação, Segurança, etc etc e etc. Tudo em um único sistema.
            </p>
          </div>
        </section>

        <section className="card-section-info bg-card-section">
          <div>
            <div>
              <img src="/icons/card-shield.svg" alt="Twitch" />
              <h5 className="text-card">Moderação</h5>
            </div>
            <div>
              <img src="/icons/card-role.svg" alt="Twitch" />
              <h5 className="text-card">Cargos</h5>
            </div>
            <div>
              <img src="/icons/card-language.svg" alt="Twitch" />
              <h5 className="text-card">Idioma</h5>
            </div>
            <div>
              <img src="/icons/card-luck.svg" alt="Twitch" />
              <h5 className="text-card">Sorteios</h5>
            </div>
            <div>
              <img src="/icons/card-twitch.svg" alt="Twitch" />
              <h5 className="text-card">Twitch</h5>
            </div>
          </div>

          <div>
            <div>
              <div className="bg-card">
                <p className="text-card-section">400</p>
              </div>
              <h5 className="text-card">Servidores</h5>
            </div>
            <div>
              <div className="bg-card">
                <p className="text-card-section">20</p>
              </div>
              <h5 className="text-card">Comandos</h5>
            </div>
          </div>
        </section>

        <section className='card-section-links'>
          <div>
            <a href="">Convide-me</a>
          </div>
          <div>
            <a target="_blank" href=""><img src="/icons/discord.svg" alt="Github"/></a>
            <a target="_blank" href=''><img src="/icons/github.svg" alt="Github"/></a>
          </div>
        </section>
      </div>
    </>
  )
}
