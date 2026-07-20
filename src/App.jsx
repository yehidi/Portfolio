import React from 'react'
import Header from './components/header/header'
import Nav from './components/nav/Nav'
import About from './components/about/about'
import Experience from './components/experience/experience'
import Portfolios from './components/portfolio/portfolio'
import Testimonials from './components/testimonials/testimonials'
import Contact from './components/contact/contact'
import Footer from './components/footer/footer'


const App = () => {
  return (
    <>
    <Header />
    <Nav />
    <About />
    <Experience />
    {/* <Services /> */}
    <Portfolios />
    {/* <Testimonials /> */}
    <Contact />
    <Footer />
    </>
  )
}

export default App