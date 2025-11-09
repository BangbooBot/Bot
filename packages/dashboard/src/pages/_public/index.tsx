import { createFileRoute } from '@tanstack/react-router'
import logo from '@/logo.svg'
import Card from '@/components/RiduCard'

export const Route = createFileRoute('/_public/')({
  component: App,
})

function App() {
  return (
    <div className="min-h-[calc(100vh-80px)] py-4 flex flex-col items-center justify-center">
      <Card />
    </div>
  )
}
