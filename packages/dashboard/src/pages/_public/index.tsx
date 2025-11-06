import { createFileRoute } from '@tanstack/react-router'
import logo from '@/logo.svg'
import Card from '@/components/RiduCard'

export const Route = createFileRoute('/_public/')({
  component: App,
})

function App() {
  return (
    <div className="text-center">
      <header className="min-h-screen flex flex-col items-center justify-center text-white text-[calc(10px+2vmin)]">
        <Card />
      </header>
    </div>
  )
}
