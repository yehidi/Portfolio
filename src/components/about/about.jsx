import React from 'react'
import './About.css'
import ME from '../../assets/me-about.jpg'
import { FaAward } from 'react-icons/fa'
import { FiUsers } from 'react-icons/fi'
import {VscFolderLibrary} from 'react-icons/vsc'

const About = () => {
  return (
    <section id='about'>
      <h5>Get To Know</h5>
      <h2>About Me</h2>

      <div className='container about__container'>
        <div className='about__me'>
          <div className='about__me-image'>
            <img src={ME} alt="About Image" />
          </div>
        </div>

        <div className='about__content'>
          <div className='about__cards'>
            <article className='about__card'>
              <FaAward className='about__icon' />
              <h5>Experience</h5>
              <small>4+ Years working </small>
            </article>

            <article className='about__card'>
              <FiUsers className='about__icon' />
              <h5>Clients</h5>
              <small>300+ Worldwide </small>
            </article>

            <article className='about__card'>
              <VscFolderLibrary className='about__icon' />
              <h5>Projects</h5>
              <small>5+ Completed </small>
            </article>
          </div>

          <p>My name is Omri, 27 years old. <br />
          Bachelor of Science in Computer Science. <br />
          I studied at the College of Management with an FSD specialization. <br />
          I have been working for a startup company for close to a year, <br />
          and I'm looking for my next challenge in a development job at a stable company where I can advance and enrich my knowledge of additional languages and technologies. <br />
          I would be happy for suggestions, you can contact one of the social networks at the bottom of the page or leave a comment at the bottom of the site.</p>

          <a href="#contact" className='btn btn-primary'>Let's Talk</a>
        </div>
      </div>
    </section>
  )
}

export default About