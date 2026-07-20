import React from 'react'
import './Footer.css'
import { FaFacebookF } from 'react-icons/fa'
import { FiInstagram } from 'react-icons/fi'
import { FaGithub } from 'react-icons/fa'
import { FaLinkedin } from 'react-icons/fa'

const footer = () => {
  return (
    <footer>
      
      <a href='#' className='footer__logo'>Yehidi</a>
      <ul className='permalinks'>
        <li><a href='#'>Home</a></li>
        <li><a href='#about'>About</a></li>
        <li><a href='#experience'>Experience</a></li>
        <li><a href='#portfolio'>Portfolio</a></li>
        <li><a href='#contact'>Contact</a></li>
      </ul>
      
      <div className='footer__socials'>
        <a href='https://facebook.com/OmriYehidi'><FaFacebookF /></a>
        <a href='https://www.instagram.com/omriyehidi'><FiInstagram /></a>
        <a href='https://github.com/yehidi'><FaGithub /></a>
        <a href='https://linkedin.com/in/omriyehidi'><FaLinkedin /></a>
      </div>

      <div className='footer__copyright'>
        <small>&copy; Yehidi. All rights reserved.</small>
      </div>
      </footer>
  )
}

export default footer