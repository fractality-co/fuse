<a name="readme-top"></a>

<!-- PROJECT SHIELDS -->
<!--
*** I'm using markdown "reference style" links for readability.
*** Reference links are enclosed in brackets [ ] instead of parentheses ( ).
*** See the bottom of this document for the declaration of the reference variables
*** for contributors-url, forks-url, etc. This is an optional, concise syntax you may use.
*** https://www.markdownguide.org/basic-syntax/#reference-style-links
-->
[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![MIT License][license-shield]][license-url]
[![LinkedIn][linkedin-shield]][linkedin-url]

<br/>

<div align="center">
  <img src="Assets/Plugins/Fuse/2D/Icons/FuseFull.png" alt="Logo" width="500">
  <p align="center">
    Dependency Injection Framework for Unity
    <br />
  </p>
</div>

<details>
  <summary>Table of Contents</summary>
  <ol>
    <li><a href="#about-the-project">About The Project</a></li>
    <li><a href="#usage">Usage</a></li>
    <li><a href="#contributing">Contributing</a></li>
    <li><a href="#license">License</a></li>
    <li><a href="#contact">Contact</a></li>
  </ol>
</details>

<!-- ABOUT THE PROJECT -->
## About The Project

FUSE is a framework for the Unity engine aimed at abstracting away the functionality you need on every single project. Whether it's an application, XR, or a game, it's meant to be light-weight and flexible to allow for usage on any possible projects.

It's architectural aim is to inverse control of development to provide structured access that sets you up for success, and scalability. After using Unity for 7+ years, this consolidates the core features needed on every project while exposing streamlined configuration.

Existing feature set:
* Provides full GUI management of all core features you'd need, with most requiring no code to configure
* Implements a fully customizable injection layer leveraging metadata (attributes) that abstracts away heaps of functionality
* An optional application layer of logic which provides global availability of systems that run independent of any scene
* State machine that manages all resources (scenes, bundles, logic) that is controlled via a simple `Events` or via injection
* Interface for managing all resources needed that automatically packages all of your bundles, and scenes in a scalable manner for either baked or online access
* Advanced build and asset pipeline that allows for more control which already comes UCB and automation ready
* Full environment management to abstract away all those pesky staging dependent URLs/settings (e.g. prod vs dev)
* Deeply documented internally with a 20+ page manual included to explain everything you'll need

Lastly, a distinct feature of the framework is that it provides easy access to all features without pushing you into any specific pattern. This makes it useful for acting as a framework container with Unity, allowing users to define their own architectural patterns that fit their needs.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Usage

FUSE can be installed either via Unity's package manager with the URL of this repository (recommended). Optionally, you can download the source code then copying the Plugins/FUSE folder into your project.

Once the framework has been added to the project, on first time installation the project will automatically bootstrap and welcome you with instructions on how to get started.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Contributing

Contributions are what make the open source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

If you have a suggestion that would make this better, please fork the repo and create a pull request. You can also simply open an issue with the tag "enhancement".
Don't forget to give the project a star! Thanks again!

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/amazing-feature`)
3. Commit your Changes (`git commit -m 'Add some amazing feature'`)
4. Push to the Branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## License

Distributed under the MIT License. See `LICENSE.md` for more information.
<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Contact

Jacob D. Harrison - this@jacobdharrison.com</br>
Project Link: [https://github.com/jdharrison/fuse](https://github.com/jdharrison/fuse)

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- https://www.markdownguide.org/basic-syntax/#reference-style-links -->
[contributors-shield]: https://img.shields.io/github/contributors/jdharrison/fuse.svg?style=for-the-badge
[contributors-url]: https://github.com/jdharrison/fuse/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/jdharrison/fuse.svg?style=for-the-badge
[forks-url]: https://github.com/jdharrison/fuse/network/members
[stars-shield]: https://img.shields.io/github/stars/jdharrison/fuse.svg?style=for-the-badge
[stars-url]: https://github.com/jdharrison/fuse/stargazers
[issues-shield]: https://img.shields.io/github/issues/jdharrison/fuse.svg?style=for-the-badge
[issues-url]: https://github.com/jdharrison/fuse/issues
[license-shield]: https://img.shields.io/github/license/jdharrison/fuse.svg?style=for-the-badge
[license-url]: https://github.com/jdharrison/fuse/blob/main/LICENSE.md
[linkedin-shield]: https://img.shields.io/badge/-LinkedIn-black.svg?style=for-the-badge&logo=linkedin&colorB=555
[linkedin-url]: https://linkedin.com/in/jacobdharrison
[product-screenshot]: images/screenshot.png
