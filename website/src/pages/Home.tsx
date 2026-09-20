import Hero from "../components/Hero";
import ProductShowcase from "../components/ProductShowcase";
import Features from "../components/Features";
import HowItWorks from "../components/HowItWorks";
import GettingStarted from "../components/GettingStarted";
import OBSSection from "../components/OBSSection";
import AppShowcase from "../components/AppShowcase";
import AutoUpdate from "../components/AutoUpdate";
import FinalCTA from "../components/FinalCTA";

export default function Home() {
  return (
    <main>
      <Hero />
      <ProductShowcase />
      <Features />
      <HowItWorks />
      <AppShowcase />
      <OBSSection />
      <GettingStarted />
      <AutoUpdate />
      <FinalCTA />
    </main>
  );
}
