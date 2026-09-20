import { Routes, Route } from "react-router-dom";
import Home from "./pages/Home";
import Download from "./pages/Download";
import Navbar from "./components/Navbar";
import Footer from "./components/Footer";

export default function App() {
  return (
    <div className="min-h-screen bg-bg text-text-primary">
      <Navbar />
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/download" element={<Download />} />
      </Routes>
      <Footer />
    </div>
  );
}
