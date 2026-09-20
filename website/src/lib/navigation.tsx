import { useNavigate } from "react-router-dom";

const NAVBAR_HEIGHT = 80;

function scrollToElement(el: HTMLElement) {
  const top = el.getBoundingClientRect().top + window.scrollY - NAVBAR_HEIGHT;
  window.scrollTo({ top, behavior: "smooth" });
}

function scrollToHashAfterNavigate(hash: string) {
  const id = hash.replace("#", "");
  let attempts = 0;
  const interval = setInterval(() => {
    attempts++;
    const el = document.getElementById(id);
    if (el || attempts > 60) {
      clearInterval(interval);
      if (el) scrollToElement(el);
    }
  }, 50);
}

export function useNavigateToSection() {
  const navigate = useNavigate();

  return (sectionId: string) => {
    const hash = `#${sectionId}`;

    if (window.location.pathname === "/") {
      const el = document.getElementById(sectionId);
      if (el) scrollToElement(el);
      return;
    }

    navigate("/");
    scrollToHashAfterNavigate(hash);
  };
}

export function useNavigateHome() {
  const navigate = useNavigate();

  return () => {
    if (window.location.pathname === "/") {
      window.scrollTo({ top: 0, behavior: "smooth" });
      return;
    }
    navigate("/");
    let attempts = 0;
    const interval = setInterval(() => {
      attempts++;
      if (document.getElementById("root") || attempts > 60) {
        clearInterval(interval);
        window.scrollTo({ top: 0, behavior: "smooth" });
      }
    }, 50);
  };
}
